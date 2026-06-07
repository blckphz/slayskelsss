using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public event Action OnInventoryChanged;

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;
        public float currentDurability;

        public InventorySlot(ItemData d = null, int c = 0, float dur = 0)
        {
            item = d;
            count = c;
            currentDurability = dur;
        }
    }

    [Header("Inventory Data")]
    public List<InventorySlot> inventory = new List<InventorySlot>();
    public List<HotbarSlotData> hotbarData = new List<HotbarSlotData>();
    public int inventorySize = 30;
    public int hotbarSize = 9;

    [Header("Dependencies")]
    public ItemDatabase database;
    public AbilityDatabase abilityDatabase;

    private string savePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/inventory.json";
        InitializeInventory();
        InitializeHotbarData();
        LoadInventory();
    }

    void InitializeInventory()
    {
        while (inventory.Count < inventorySize)
            inventory.Add(new InventorySlot());
    }

    void InitializeHotbarData()
    {
        if (hotbarData == null) hotbarData = new List<HotbarSlotData>();
        if (hotbarData.Count != hotbarSize)
        {
            hotbarData.Clear();
            for (int i = 0; i < hotbarSize; i++)
                hotbarData.Add(new HotbarSlotData());
        }
    }

    // Determines if an item can merge into an existing slot
    bool CanStack(ItemData data, InventorySlot slot, float durability)
    {
        if (slot.item == null || slot.item.itemID != data.itemID) return false;

        // Non-durable items always stack if ID matches
        if (data.maxDurability <= 0) return true;

        // Durable items only stack if BOTH are at max durability
        bool isSlotFull = Mathf.Approximately(slot.currentDurability, data.maxDurability);
        bool isIncomingFull = Mathf.Approximately(durability, data.maxDurability);

        return isSlotFull && isIncomingFull;
    }

    public void AddItem(ItemData data, int amount, float customDurability = -1f)
    {
        if (data == null || amount <= 0) return;

        float durability = customDurability < 0 ? data.maxDurability : customDurability;

        // Unified stacking logic for all items
        StackItem(data, amount, durability);

        RefreshAll();
        OnInventoryChanged?.Invoke();
    }

    void StackItem(ItemData data, int amount, float durability)
    {
        int remaining = amount;

        // 1. Fill existing stacks
        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            if (CanStack(data, inventory[i], durability))
            {
                int space = data.maxStackSize - inventory[i].count;
                int add = Mathf.Min(space, remaining);
                if (add > 0)
                {
                    inventory[i].count += add;
                    remaining -= add;
                }
            }
        }

        // 2. Fill empty slots
        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            if (inventory[i].item == null)
            {
                int add = Mathf.Min(remaining, data.maxStackSize);
                inventory[i].item = data;
                inventory[i].count = add;
                inventory[i].currentDurability = durability;
                remaining -= add;
            }
        }

        if (remaining > 0)
            Debug.LogWarning($"[INV] Inventory full! Lost {remaining}x {data.itemName}");
    }

    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;
        int remaining = amount;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            var slot = inventory[i];
            if (slot.item == null || slot.item.itemID != data.itemID) continue;

            int take = Mathf.Min(slot.count, remaining);
            slot.count -= take;
            remaining -= take;

            if (slot.count <= 0)
            {
                slot.item = null;
                slot.currentDurability = 0f;
            }
            if (remaining <= 0) break;
        }

        RefreshAll();
        OnInventoryChanged?.Invoke();
        return remaining <= 0;
    }

    // Helper methods for crafting/queries
    public int GetTotalCount(ItemData data)
    {
        int total = 0;
        foreach (var s in inventory) if (s.item?.itemID == data.itemID) total += s.count;
        foreach (var h in hotbarData) if (h.item?.itemID == data.itemID) total += h.count;
        return total;
    }

    public void RefreshAll()
    {
        InvUI.Instance?.RefreshUI();
        SaveInventory();
    }

    public void SaveInventory()
    {
        PlayerHotbarManager.Instance?.SyncHotbarToData();
        InventorySaveData save = new InventorySaveData();

        foreach (var s in inventory)
            save.savedItems.Add(new SaveSlot { itemId = s.item ? s.item.itemID : -1, count = s.count, currentDurability = s.currentDurability });

        foreach (var h in hotbarData)
            save.hotbarItems.Add(new HotbarSaveSlot { itemId = h.item ? h.item.itemID : -1, abilityName = h.ability ? h.ability.name : "", count = h.count, currentDurability = h.currentDurability });

        File.WriteAllText(savePath, JsonUtility.ToJson(save, true));
    }

    void LoadInventory()
    {
        if (!File.Exists(savePath)) return;
        var data = JsonUtility.FromJson<InventorySaveData>(File.ReadAllText(savePath));

        InitializeInventory();
        for (int i = 0; i < inventory.Count && i < data.savedItems.Count; i++)
        {
            var s = data.savedItems[i];
            inventory[i].item = database.GetItemByID(s.itemId);
            inventory[i].count = s.count;
            inventory[i].currentDurability = s.currentDurability;
        }

        InitializeHotbarData();
        for (int i = 0; i < hotbarSize && i < data.hotbarItems.Count; i++)
        {
            var h = data.hotbarItems[i];
            hotbarData[i].item = database.GetItemByID(h.itemId);
            hotbarData[i].ability = abilityDatabase.GetAbilityByName(h.abilityName);
            hotbarData[i].count = h.count;
            hotbarData[i].currentDurability = h.currentDurability;
        }
        PlayerHotbarManager.Instance?.RefreshHotbar();
    }

    // =========================================================
    // CRAFTING API EXTENSIONS
    // =========================================================

    public List<ItemInstance> GetItems(ItemData data)
    {
        List<ItemInstance> result = new List<ItemInstance>();
        if (data == null) return result;

        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                result.Add(new ItemInstance(slot.item, slot.count)
                {
                    CurrentDurability = slot.currentDurability
                });
            }
        }
        return result;
    }

    public void RemoveItemsWithCondition(ItemData data, int amount, Predicate<ItemInstance> condition)
    {
        if (data == null) return;

        int remaining = amount;
        // Iterate backwards so we can safely null out slots if needed
        for (int i = inventory.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = inventory[i];

            if (slot.item == null || slot.item.itemID != data.itemID) continue;

            ItemInstance inst = new ItemInstance(slot.item, slot.count)
            {
                CurrentDurability = slot.currentDurability
            };

            // Only consume if the item meets the craft/condition requirement
            if (!condition(inst)) continue;

            int take = Mathf.Min(slot.count, remaining);
            slot.count -= take;
            remaining -= take;

            if (slot.count <= 0)
            {
                slot.item = null;
                slot.currentDurability = 0f;
            }
        }

        RefreshAll();
        OnInventoryChanged?.Invoke();
    }

}