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

        if (hotbarData.Count == 0)
        {
            for (int i = 0; i < hotbarSize; i++)
                hotbarData.Add(new HotbarSlotData());
        }

        LoadInventory();
    }

    // =========================
    // ADD ITEM
    // =========================
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

    // =========================
    // REMOVE ITEM
    // =========================
    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;

        bool changed = false;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, amount);
                inventory[i].count -= take;
                amount -= take;

                if (inventory[i].count <= 0)
                    inventory.RemoveAt(i);

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

                    if (hotbarData[i].count <= 0)
                        hotbarData[i].Clear();

                    changed = true;
                    if (amount <= 0) break;
                }
            }
        }

        if (changed) RefreshAll();
        return changed;
    }

    // =========================
    // 🔥 FIX: THIS WAS MISSING
    // =========================
    public void ConsumeResources(ItemData data, int amount)
    {
        if (data == null) return;

        int remaining = amount;

        // inventory first
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, remaining);
                inventory[i].count -= take;
                remaining -= take;

                if (inventory[i].count <= 0)
                    inventory.RemoveAt(i);

                if (remaining <= 0) break;
            }
        }

        // hotbar fallback
        if (remaining > 0)
        {
            for (int i = 0; i < hotbarData.Count; i++)
            {
                if (hotbarData[i].item != null && hotbarData[i].item.itemID == data.itemID)
                {
                    int take = Mathf.Min(hotbarData[i].count, remaining);
                    hotbarData[i].count -= take;
                    remaining -= take;

                    if (hotbarData[i].count <= 0)
                        hotbarData[i].Clear();

                    if (remaining <= 0) break;
                }
            }
        }

        RefreshAll();
    }

    // =========================
    // SAVE
    // =========================
    public void SaveInventory()
    {
        InventorySaveData data = new InventorySaveData();

        foreach (var slot in inventory)
        {
            if (slot.item != null)
                data.savedItems.Add(new SaveSlot
                {
                    itemId = slot.item.itemID,
                    count = slot.count
                });
        }

        for (int i = 0; i < hotbarData.Count; i++)
        {
            var h = hotbarData[i];

            data.hotbarItems.Add(new HotbarSaveSlot
            {
                itemId = h.item != null ? h.item.itemID : -1,
                abilityName = h.ability != null ? h.ability.name : "",
                count = h.count
            });
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    // =========================
    // LOAD
    // =========================
    public void LoadInventory()
    {
        if (!File.Exists(savePath)) return;

        var data = JsonUtility.FromJson<InventorySaveData>(File.ReadAllText(savePath));

        inventory.Clear();

        foreach (var s in data.savedItems)
        {
            ItemData item = database.GetItemByID(s.itemId);
            if (item != null)
                inventory.Add(new InventorySlot(item, s.count));
        }

        for (int i = 0; i < hotbarSize; i++)
        {
            if (i >= data.hotbarItems.Count) continue;

            var h = data.hotbarItems[i];

            hotbarData[i].Clear();
            hotbarData[i].count = h.count;

            if (h.itemId != -1)
                hotbarData[i].item = database.GetItemByID(h.itemId);

            if (!string.IsNullOrEmpty(h.abilityName))
                hotbarData[i].ability = abilityDatabase.GetAbilityByName(h.abilityName);
        }
    }

    public void RefreshAll()
    {
        SaveInventory();
        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();
    }

    public int GetTotalCount(ItemData data)
    {
        int total = 0;

        foreach (var s in inventory)
            if (s.item == data) total += s.count;

        foreach (var h in hotbarData)
            if (h.item == data) total += h.count;

        return total;
    }
}