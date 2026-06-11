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
        public int currentDurability;

        public InventorySlot(ItemData d = null, int c = 0, int dur = 0)
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
        if (hotbarData == null)
            hotbarData = new List<HotbarSlotData>();

        if (hotbarData.Count != hotbarSize)
        {
            hotbarData.Clear();
            for (int i = 0; i < hotbarSize; i++)
                hotbarData.Add(new HotbarSlotData());
        }
    }

    // =========================================================
    // STACK CHECK
    // =========================================================
    bool CanStack(ItemData data, InventorySlot slot, float durability)
    {
        if (slot.item == null) return false;
        if (slot.item.itemID != data.itemID) return false;

        if (data.maxDurability <= 0) return true;

        return Mathf.Approximately(slot.currentDurability, durability);
    }

    // =========================================================
    // ADD ITEM (FIXED SIGNATURE)
    // =========================================================
    public void AddItem(ItemData data, int amount, float customDurability = -1f)
    {
        if (data == null || amount <= 0) return;

        float durability = Mathf.Max(customDurability, data.maxDurability);

        StackItem(data, amount, durability);

        RefreshAll();
        OnInventoryChanged?.Invoke();
    }

    // =========================================================
    // STACKING
    // =========================================================
    void StackItem(ItemData data, int amount, float durability)
    {
        int remaining = amount;

        // Fill existing stacks
        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            var slot = inventory[i];

            if (CanStack(data, slot, durability))
            {
                int space = data.maxStackSize - slot.count;

                if (space > 0)
                {
                    int add = Mathf.Min(space, remaining);

                    slot.count += add;
                    remaining -= add;
                }
            }
        }

        // Empty slots
        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            var slot = inventory[i];

            if (slot.item == null)
            {
                int add = Mathf.Min(remaining, data.maxStackSize);

                slot.item = data;
                slot.count = add;
                slot.currentDurability = (int)durability;

                remaining -= add;
            }
        }

        if (remaining > 0)
        {
            Debug.LogWarning("[Inventory] Not enough space for items!");
        }
    }

    // =========================================================
    // REMOVE ITEM
    // =========================================================
    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;

        int remaining = amount;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            var slot = inventory[i];

            if (slot.item == null) continue;
            if (slot.item.itemID != data.itemID) continue;

            int take = Mathf.Min(slot.count, remaining);

            slot.count -= take;
            remaining -= take;

            if (slot.count <= 0)
            {
                slot.item = null;
                slot.currentDurability = 0;
            }

            if (remaining <= 0)
                break;
        }

        RefreshAll();
        OnInventoryChanged?.Invoke();
        return remaining <= 0;
    }

    // =========================================================
    // TOTAL COUNT
    // =========================================================
    public int GetTotalCount(ItemData data)
    {
        int total = 0;

        foreach (var s in inventory)
            if (s.item?.itemID == data.itemID)
                total += s.count;

        foreach (var h in hotbarData)
            if (h.item?.itemID == data.itemID)
                total += h.count;

        return total;
    }

    // =========================================================
    // REFRESH
    // =========================================================
    public void RefreshAll()
    {
        InvUI.Instance?.RefreshUI();
        SaveInventory();
    }

    // =========================================================
    // SAVE
    // =========================================================
    public void SaveInventory()
    {
        PlayerHotbarManager.Instance?.SyncHotbarToData();

        InventorySaveData save = new InventorySaveData();

        foreach (var s in inventory)
        {
            save.savedItems.Add(new SaveSlot
            {
                itemId = s.item ? s.item.itemID : -1,
                count = s.count,
                currentDurability = s.currentDurability
            });
        }

        foreach (var h in hotbarData)
        {
            save.hotbarItems.Add(new HotbarSaveSlot
            {
                itemId = h.item ? h.item.itemID : -1,
                abilityName = h.ability ? h.ability.name : "",
                count = h.count,
                currentDurability = h.currentDurability
            });
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(save, true));
    }

    // =========================================================
    // LOAD
    // =========================================================
    void LoadInventory()
    {
        if (!File.Exists(savePath)) return;

        var data = JsonUtility.FromJson<InventorySaveData>(
            File.ReadAllText(savePath)
        );

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
    // CRAFTING SUPPORT
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

        for (int i = inventory.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = inventory[i];

            if (slot.item == null || slot.item.itemID != data.itemID)
                continue;

            ItemInstance inst = new ItemInstance(slot.item, slot.count)
            {
                CurrentDurability = slot.currentDurability
            };

            if (!condition(inst))
                continue;

            int take = Mathf.Min(slot.count, remaining);

            slot.count -= take;
            remaining -= take;

            if (slot.count <= 0)
            {
                slot.item = null;
                slot.currentDurability = 0;
            }
        }

        RefreshAll();
        OnInventoryChanged?.Invoke();
    }
}