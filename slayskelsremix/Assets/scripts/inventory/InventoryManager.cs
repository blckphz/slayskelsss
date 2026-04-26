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

    public void AddItem(ItemData data, int amount)
    {
        if (data == null) return;
        foreach (var hSlot in hotbarData)
        {
            if (hSlot.item != null && hSlot.item.itemID == data.itemID)
            {
                hSlot.count += amount;
                RefreshAll();
                return;
            }
        }
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

    // FIXED: Added back the RemoveItem definition
    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;
        bool changed = false;

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

        if (changed) RefreshAll();
        return changed;
    }

    public void ConsumeResources(ItemData data, int amount)
    {
        int remaining = amount;
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, remaining);
                inventory[i].count -= take;
                remaining -= take;
                if (inventory[i].count <= 0) inventory.RemoveAt(i);
            }
            if (remaining <= 0) break;
        }
        if (remaining > 0)
        {
            foreach (var hSlot in hotbarData)
            {
                if (hSlot.item != null && hSlot.item.itemID == data.itemID)
                {
                    int take = Mathf.Min(hSlot.count, remaining);
                    hSlot.count -= take;
                    remaining -= take;
                    if (hSlot.count <= 0) hSlot.Clear();
                }
                if (remaining <= 0) break;
            }
        }
        RefreshAll();
    }

    public int GetTotalCount(ItemData data)
    {
        int total = 0;
        foreach (var slot in inventory) if (slot.item != null && slot.item.itemID == data.itemID) total += slot.count;
        foreach (var hSlot in hotbarData) if (hSlot.item != null && hSlot.item.itemID == data.itemID) total += hSlot.count;
        return total;
    }

    public void RefreshAll()
    {
        SaveInventory();
        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();
    }

    public void SaveInventory()
    {
        InventorySaveData data = new InventorySaveData();
        foreach (var slot in inventory) if (slot.item != null) data.savedItems.Add(new SaveSlot { itemId = slot.item.itemID, count = slot.count });
        foreach (var hSlot in hotbarData) data.hotbarItems.Add(new HotbarSaveSlot
        {
            itemId = hSlot.item != null ? hSlot.item.itemID : -1,
            abilityName = hSlot.ability != null ? hSlot.ability.name : "",
            count = hSlot.count
        });
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    public void LoadInventory()
    {
        if (!File.Exists(savePath)) return;
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(File.ReadAllText(savePath));
        inventory.Clear();
        foreach (var s in data.savedItems)
        {
            ItemData item = database.GetItemByID(s.itemId);
            if (item != null) inventory.Add(new InventorySlot(item, s.count));
        }
        for (int i = 0; i < hotbarSize; i++)
        {
            if (i < data.hotbarItems.Count)
            {
                var h = data.hotbarItems[i];
                hotbarData[i].Clear();
                hotbarData[i].count = h.count;
                if (h.itemId != -1) hotbarData[i].item = database.GetItemByID(h.itemId);
                if (!string.IsNullOrEmpty(h.abilityName)) hotbarData[i].ability = abilityDatabase.GetAbilityByName(h.abilityName);
            }
        }
    }
}

// FIXED: Added missing data classes back so they are globally accessible
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