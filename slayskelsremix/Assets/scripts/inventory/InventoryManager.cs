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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/inventory.json";

        InitializeInventory();
        InitializeHotbarData();
        LoadInventory();
    }

    private void InitializeInventory()
    {
        while (inventory.Count < inventorySize)
            inventory.Add(new InventorySlot());
    }

    private void InitializeHotbarData()
    {
        if (hotbarData == null) hotbarData = new List<HotbarSlotData>();

        if (hotbarData.Count != hotbarSize)
        {
            hotbarData.Clear();
            for (int i = 0; i < hotbarSize; i++)
                hotbarData.Add(new HotbarSlotData());
        }
    }

    public void AddItem(ItemData data, int amount, float customDurability = -1f)
    {
        if (data == null || amount <= 0) return;

        // stackable
        if (data.maxDurability <= 0)
        {
            int remaining = amount;

            // fill existing stacks
            for (int i = 0; i < inventory.Count; i++)
            {
                var slot = inventory[i];
                if (slot.item != null && slot.item.itemID == data.itemID && slot.count < data.maxStackSize)
                {
                    int space = data.maxStackSize - slot.count;
                    int add = Mathf.Min(space, remaining);

                    slot.count += add;
                    remaining -= add;

                    if (remaining <= 0) break;
                }
            }

            // new slots
            for (int i = 0; i < inventory.Count && remaining > 0; i++)
            {
                if (inventory[i].item == null)
                {
                    int add = Mathf.Min(remaining, data.maxStackSize);
                    inventory[i].item = data;
                    inventory[i].count = add;
                    inventory[i].currentDurability = 0f;

                    remaining -= add;
                }
            }
        }
        else
        {
            // durability items = 1 per slot
            for (int i = 0; i < inventory.Count && amount > 0; i++)
            {
                if (inventory[i].item == null)
                {
                    float dur = (customDurability < 0) ? data.maxDurability : customDurability;
                    inventory[i].item = data;
                    inventory[i].count = 1;
                    inventory[i].currentDurability = dur;
                    amount--;
                }
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
            var slot = inventory[i];

            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                int take = Mathf.Min(slot.count, remaining);
                slot.count -= take;
                remaining -= take;

                if (slot.count <= 0)
                {
                    slot.item = null;
                    slot.currentDurability = 0;
                }

                if (remaining <= 0) break;
            }
        }

        RefreshAll();
        return true;
    }

    public void SaveInventory()
    {
        PlayerHotbarManager.Instance?.SyncHotbarToData();

        InventorySaveData saveData = new InventorySaveData();

        foreach (var slot in inventory)
        {
            saveData.savedItems.Add(new SaveSlot
            {
                itemId = slot.item != null ? slot.item.itemID : -1,
                count = slot.count,
                currentDurability = slot.currentDurability
            });
        }

        foreach (var h in hotbarData)
        {
            saveData.hotbarItems.Add(new HotbarSaveSlot
            {
                itemId = h.item != null ? h.item.itemID : -1,
                abilityName = h.ability != null ? h.ability.name : "",
                count = h.count,
                currentDurability = h.currentDurability
            });
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(saveData, true));
    }

    public void LoadInventory()
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

    public void RefreshAll()
    {
        SaveInventory();
        InvUI.Instance?.RefreshUI();
    }

    public int GetTotalCount(ItemData data)
    {
        if (data == null) return 0;

        int total = 0;

        // inventory
        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
                total += slot.count;
        }

        // hotbar
        foreach (var h in hotbarData)
        {
            if (h.item != null && h.item.itemID == data.itemID)
                total += h.count;
        }

        return total;
    }
}