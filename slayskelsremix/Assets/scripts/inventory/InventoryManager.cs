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
        public float currentDurability;

        public InventorySlot(ItemData d, int c, float dur)
        {
            item = d;
            count = c;
            currentDurability = dur;
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
        InitializeHotbarData();
        LoadInventory();
    }

    private void InitializeHotbarData()
    {
        if (hotbarData == null) hotbarData = new List<HotbarSlotData>();
        if (hotbarData.Count != hotbarSize)
        {
            hotbarData.Clear();
            for (int i = 0; i < hotbarSize; i++) hotbarData.Add(new HotbarSlotData());
        }
    }

    public void AddItem(ItemData data, int amount, float customDurability = -1f)
    {
        if (data == null || amount <= 0) return;
        if (data.maxDurability > 0)
        {
            for (int i = 0; i < amount; i++)
            {
                float dur = (customDurability < 0) ? data.maxDurability : customDurability;
                inventory.Add(new InventorySlot(data, 1, dur));
            }
        }
        else
        {
            int remaining = amount;
            foreach (var slot in inventory)
            {
                if (slot.item != null && slot.item.itemID == data.itemID && slot.count < data.maxStackSize)
                {
                    int space = data.maxStackSize - slot.count;
                    int add = Mathf.Min(space, remaining);
                    slot.count += add;
                    remaining -= add;
                    if (remaining <= 0) break;
                }
            }
            while (remaining > 0)
            {
                int next = Mathf.Min(remaining, data.maxStackSize);
                inventory.Add(new InventorySlot(data, next, 0f));
                remaining -= next;
            }
        }
        RefreshAll();
    }

    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;
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
        RefreshAll();
        return true;
    }

    public int GetTotalCount(ItemData data)
    {
        int total = 0;
        foreach (var s in inventory) if (s.item != null && s.item.itemID == data.itemID) total += s.count;
        foreach (var h in hotbarData) if (h.item != null && h.item.itemID == data.itemID) total += h.count;
        return total;
    }

    public void SaveInventory()
    {
        PlayerHotbarManager.Instance?.SyncHotbarToData();
        InventorySaveData saveData = new InventorySaveData();
        foreach (var slot in inventory)
            if (slot.item != null) saveData.savedItems.Add(new SaveSlot { itemId = slot.item.itemID, count = slot.count, currentDurability = slot.currentDurability });

        foreach (var h in hotbarData)
            saveData.hotbarItems.Add(new HotbarSaveSlot { itemId = h.item != null ? h.item.itemID : -1, abilityName = h.ability != null ? h.ability.name : "", count = h.count, currentDurability = h.currentDurability });

        File.WriteAllText(savePath, JsonUtility.ToJson(saveData, true));
    }

    public void LoadInventory()
    {
        if (!File.Exists(savePath)) return;
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(File.ReadAllText(savePath));
        inventory.Clear();
        foreach (var s in data.savedItems)
        {
            ItemData item = database.GetItemByID(s.itemId);
            if (item != null) inventory.Add(new InventorySlot(item, s.count, s.currentDurability));
        }
        InitializeHotbarData();
        for (int i = 0; i < hotbarSize && i < data.hotbarItems.Count; i++)
        {
            var h = data.hotbarItems[i];
            hotbarData[i].item = database.GetItemByID(h.itemId);
            hotbarData[i].ability = (abilityDatabase != null) ? abilityDatabase.GetAbilityByName(h.abilityName) : null;
            hotbarData[i].count = h.count;
            hotbarData[i].currentDurability = h.currentDurability;
        }
        PlayerHotbarManager.Instance?.RefreshHotbar();
    }

    public void RefreshAll()
    {
        SaveInventory();
        InvUI.Instance?.RefreshUI();
    }
}