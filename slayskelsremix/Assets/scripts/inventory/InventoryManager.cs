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

        public InventorySlot(ItemData d, int c)
        {
            item = d;
            count = c;
        }
    }

    [Header("Inventory Data")]
    public List<InventorySlot> inventory = new List<InventorySlot>();
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

        // 🔥 CRITICAL FIX 1: Initialize list structure BEFORE loading
        InitializeHotbarData();

        LoadInventory();
    }

    private void InitializeHotbarData()
    {
        if (hotbarData == null) hotbarData = new List<HotbarSlotData>();

        // If the list is empty or wrong size, populate it with fresh objects
        if (hotbarData.Count != hotbarSize)
        {
            hotbarData.Clear();
            for (int i = 0; i < hotbarSize; i++)
            {
                hotbarData.Add(new HotbarSlotData());
            }
        }
    }

    // ==========================================
    // ITEM MANAGEMENT
    // ==========================================

    public void AddItem(ItemData data, int amount)
    {
        if (data == null) return;

        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                slot.count += amount;
                RefreshAll();
                return;
            }
        }

        inventory.Add(new InventorySlot(data, amount));
        RefreshAll();
    }

    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;
        bool changed = false;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, amount);
                inventory[i].count -= take;
                amount -= take;
                if (inventory[i].count <= 0) inventory.RemoveAt(i);
                changed = true;
                if (amount <= 0) break;
            }
        }

        if (amount > 0)
        {
            for (int i = 0; i < hotbarData.Count; i++)
            {
                if (hotbarData[i].item != null && hotbarData[i].item.itemID == data.itemID)
                {
                    int take = Mathf.Min(hotbarData[i].count, amount);
                    hotbarData[i].count -= take;
                    amount -= take;
                    if (hotbarData[i].count <= 0) hotbarData[i].Clear();
                    changed = true;
                    if (amount <= 0) break;
                }
            }
        }

        if (changed) RefreshAll();
        return changed;
    }

    public void ConsumeResources(ItemData data, int amount)
    {
        if (data == null) return;
        int remaining = amount;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, remaining);
                inventory[i].count -= take;
                remaining -= take;
                if (inventory[i].count <= 0) inventory.RemoveAt(i);
                if (remaining <= 0) break;
            }
        }

        if (remaining > 0)
        {
            for (int i = 0; i < hotbarData.Count; i++)
            {
                if (hotbarData[i].item != null && hotbarData[i].item.itemID == data.itemID)
                {
                    int take = Mathf.Min(hotbarData[i].count, remaining);
                    hotbarData[i].count -= take;
                    remaining -= take;
                    if (hotbarData[i].count <= 0) hotbarData[i].Clear();
                    if (remaining <= 0) break;
                }
            }
        }
        RefreshAll();
    }

    // ==========================================
    // SAVE / LOAD SYSTEM
    // ==========================================

    public void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();

        foreach (var slot in inventory)
        {
            if (slot.item != null)
                saveData.savedItems.Add(new SaveSlot { itemId = slot.item.itemID, count = slot.count });
        }

        for (int i = 0; i < hotbarData.Count; i++)
        {
            var h = hotbarData[i];
            saveData.hotbarItems.Add(new HotbarSaveSlot
            {
                itemId = h.item != null ? h.item.itemID : -1,
                abilityName = h.ability != null ? h.ability.name : "",
                count = h.count
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"<color=cyan>[Inventory]</color> Data saved to JSON.");
    }

    public void LoadInventory()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        string json = File.ReadAllText(savePath);
        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

        inventory.Clear();
        foreach (var s in saveData.savedItems)
        {
            ItemData item = database.GetItemByID(s.itemId);
            if (item != null) inventory.Add(new InventorySlot(item, s.count));
        }

        // 🔥 CRITICAL FIX 2: Re-Initialize to ensure we aren't loading into null refs
        InitializeHotbarData();

        for (int i = 0; i < hotbarSize; i++)
        {
            if (i >= saveData.hotbarItems.Count) break;

            var hSave = saveData.hotbarItems[i];
            hotbarData[i].Clear(); // Reset the existing object in the list

            if (hSave.itemId != -1)
            {
                hotbarData[i].item = database.GetItemByID(hSave.itemId);
                hotbarData[i].count = hSave.count;
            }

            if (!string.IsNullOrEmpty(hSave.abilityName) && abilityDatabase != null)
            {
                hotbarData[i].ability = abilityDatabase.GetAbilityByName(hSave.abilityName);
            }
        }

        Debug.Log("<color=cyan>[Inventory]</color> Hotbar data populated from file.");
    }

    public void RefreshAll()
    {
        SaveInventory();
        InvUI.Instance?.RefreshUI();
        if (PlayerHotbarManager.Instance != null)
        {
            PlayerHotbarManager.Instance.RefreshHotbar();
        }
    }

    public int GetTotalCount(ItemData data)
    {
        int total = 0;
        foreach (var s in inventory)
            if (s.item != null && s.item.itemID == data.itemID) total += s.count;
        foreach (var h in hotbarData)
            if (h.item != null && h.item.itemID == data.itemID) total += h.count;
        return total;
    }
}