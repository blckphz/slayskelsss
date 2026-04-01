using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;
        public InventorySlot(ItemData d, int c) { item = d; count = c; }
    }

    [Header("Inventory Data")]
    public List<InventorySlot> inventory = new List<InventorySlot>();

    [Header("Hotbar Data")]
    public List<HotbarSlotData> hotbarData = new List<HotbarSlotData>();
    public int hotbarSize = 9;

    [Header("Dependencies")]
    public ItemDatabase database;
    public AbilityDatabase abilityDatabase;

    private string savePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/inventory.json";

        if (hotbarData.Count == 0)
        {
            for (int i = 0; i < hotbarSize; i++) hotbarData.Add(new HotbarSlotData());
        }

        LoadInventory();
    }

    // --- ADD ITEM LOGIC ---
    public void AddItem(ItemData data, int amount)
    {
        if (data == null) return;

        // 1. FIRST: Check if the item exists in the HOTBAR
        foreach (var hSlot in hotbarData)
        {
            if (hSlot.item != null && hSlot.item.itemID == data.itemID)
            {
                hSlot.count += amount;
                Debug.Log($"<color=cyan>[Pickup]</color> Added {amount} to Hotbar stack of {data.itemName}.");
                RefreshAll();
                return;
            }
        }

        // 2. SECOND: Check if the item exists in the MAIN INVENTORY
        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                slot.count += amount;
                Debug.Log($"<color=green>[Pickup]</color> Added {amount} to Inventory stack of {data.itemName}.");
                RefreshAll();
                return;
            }
        }

        // 3. THIRD: If it doesn't exist anywhere, add as a new stack in the inventory
        inventory.Add(new InventorySlot(data, amount));
        RefreshAll();
    }

    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;
        bool changed = false;

        // Check Inventory
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                inventory[i].count -= amount;
                if (inventory[i].count <= 0) inventory.RemoveAt(i);
                changed = true;
                break;
            }
        }

        // Check Hotbar
        if (!changed)
        {
            foreach (var hSlot in hotbarData)
            {
                if (hSlot.item != null && hSlot.item.itemID == data.itemID)
                {
                    hSlot.count -= amount;
                    if (hSlot.count <= 0) hSlot.Clear();
                    changed = true;
                    break;
                }
            }
        }

        if (changed)
        {
            RefreshAll();
            return true;
        }
        return false;
    }

    // --- CRAFTING HELPERS ---

    /// <summary>
    /// Checks the combined total of an item in both Inventory and Hotbar.
    /// Used by RecipeSO to see if crafting is possible.
    /// </summary>
    public int GetTotalCount(ItemData data)
    {
        int total = 0;
        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
                total += slot.count;
        }

        foreach (var hSlot in hotbarData)
        {
            if (hSlot.item != null && hSlot.item.itemID == data.itemID)
                total += hSlot.count;
        }

        return total;
    }

    /// <summary>
    /// Removes a specific amount of resources globally (Inv first, then Hotbar).
    /// Used when a craft is successful.
    /// </summary>
    public void ConsumeResources(ItemData data, int amount)
    {
        int remainingToRemove = amount;

        // 1. Take from Main Inventory first
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, remainingToRemove);
                inventory[i].count -= take;
                remainingToRemove -= take;

                if (inventory[i].count <= 0) inventory.RemoveAt(i);
            }
            if (remainingToRemove <= 0) break;
        }

        // 2. Take from Hotbar if still needed
        if (remainingToRemove > 0)
        {
            foreach (var hSlot in hotbarData)
            {
                if (hSlot.item != null && hSlot.item.itemID == data.itemID)
                {
                    int take = Mathf.Min(hSlot.count, remainingToRemove);
                    hSlot.count -= take;
                    remainingToRemove -= take;

                    if (hSlot.count <= 0) hSlot.Clear();
                }
                if (remainingToRemove <= 0) break;
            }
        }

        RefreshAll();
    }

    // --- REFRESH AND UI ---

    public void RefreshAll()
    {
        SaveInventory();
        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();
    }

    [ContextMenu("Save Inventory")]
    public void SaveInventory()
    {
        InventorySaveData dataToSave = new InventorySaveData();

        foreach (var slot in inventory)
        {
            if (slot.item != null)
                dataToSave.savedItems.Add(new SaveSlot { itemId = slot.item.itemID, count = slot.count });
        }

        foreach (var hSlot in hotbarData)
        {
            dataToSave.hotbarItems.Add(new HotbarSaveSlot
            {
                itemId = hSlot.item != null ? hSlot.item.itemID : -1,
                abilityName = hSlot.ability != null ? hSlot.ability.name : "",
                count = hSlot.count
            });
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(dataToSave, true));
    }

    [ContextMenu("Load Inventory")]
    public void LoadInventory()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        InventorySaveData loadedData = JsonUtility.FromJson<InventorySaveData>(json);

        if (loadedData == null) return;

        inventory.Clear();
        foreach (var savedSlot in loadedData.savedItems)
        {
            ItemData item = database.GetItemByID(savedSlot.itemId);
            if (item != null) inventory.Add(new InventorySlot(item, savedSlot.count));
        }

        for (int i = 0; i < hotbarSize; i++)
        {
            if (i < loadedData.hotbarItems.Count)
            {
                var hSaved = loadedData.hotbarItems[i];
                hotbarData[i].Clear();
                hotbarData[i].count = hSaved.count;
                if (hSaved.itemId != -1) hotbarData[i].item = database.GetItemByID(hSaved.itemId);
                if (!string.IsNullOrEmpty(hSaved.abilityName))
                    hotbarData[i].ability = abilityDatabase.GetAbilityByName(hSaved.abilityName);
            }
        }

        Invoke(nameof(RefreshUIOnly), 0.1f);
    }

    private void RefreshUIOnly()
    {
        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();
    }
}

// --- DATA PERSISTENCE CLASSES ---

[System.Serializable]
public class SaveSlot { public int itemId; public int count; }

[System.Serializable]
public class HotbarSaveSlot { public int itemId; public string abilityName; public int count; }

[System.Serializable]
public class InventorySaveData
{
    public List<SaveSlot> savedItems = new List<SaveSlot>();
    public List<HotbarSaveSlot> hotbarItems = new List<HotbarSaveSlot>();
}