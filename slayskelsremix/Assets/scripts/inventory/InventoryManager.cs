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

        public InventorySlot(ItemData item, int count, float durability)
        {
            this.item = item;
            this.count = count;
            this.currentDurability = durability;
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
        if (hotbarData == null)
            hotbarData = new List<HotbarSlotData>();

        while (hotbarData.Count < hotbarSize)
            hotbarData.Add(new HotbarSlotData());

        while (hotbarData.Count > hotbarSize)
            hotbarData.RemoveAt(hotbarData.Count - 1);
    }

    public void AddItem(ItemData data, int amount)
    {
        if (data == null || amount <= 0) return;

        if (data.maxDurability > 0)
        {
            for (int i = 0; i < amount; i++)
            {
                inventory.Add(new InventorySlot(data, 1, data.maxDurability));
            }

            RefreshAll();
            return;
        }

        int remaining = amount;

        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
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
            int add = Mathf.Min(remaining, data.maxStackSize);

            inventory.Add(new InventorySlot(data, add, 1f));
            remaining -= add;
        }

        RefreshAll();
    }

    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;

        bool changed = false;

        for (int i = inventory.Count - 1; i >= 0 && amount > 0; i--)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, amount);
                inventory[i].count -= take;
                amount -= take;
                changed = true;

                if (inventory[i].count <= 0)
                    inventory.RemoveAt(i);
            }
        }

        if (changed) RefreshAll();
        return changed;
    }

    public void DegradeEquippedToolDurability(float amount, int hotbarIndex)
    {
        if (hotbarIndex < 0 || hotbarIndex >= hotbarData.Count) return;

        var slot = hotbarData[hotbarIndex];
        if (slot == null || slot.item == null) return;

        if (slot.item.maxDurability <= 0) return;

        slot.currentDurability -= amount;
        slot.currentDurability = Mathf.Max(slot.currentDurability, 0f);

        if (slot.currentDurability <= 0f)
        {
            slot.count--;

            if (slot.count <= 0)
            {
                slot.Clear();
            }
            else
            {
                slot.currentDurability = slot.item.maxDurability;
            }
        }

        RefreshAll();
    }

    public void SaveInventory()
    {
        InventorySaveData save = new InventorySaveData();

        foreach (var s in inventory)
        {
            if (s.item == null) continue;

            save.savedItems.Add(new SaveSlot
            {
                itemId = s.item.itemID,
                count = s.count,
                currentDurability = s.currentDurability
            });
        }

        for (int i = 0; i < hotbarData.Count; i++)
        {
            var h = hotbarData[i];

            save.hotbarItems.Add(new HotbarSaveSlot
            {
                itemId = h.item != null ? h.item.itemID : -1,
                abilityName = h.ability != null ? h.ability.name : "",
                count = h.count,
                currentDurability = h.currentDurability
            });
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(save, true));
    }

    public void LoadInventory()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        InventorySaveData save = JsonUtility.FromJson<InventorySaveData>(json);

        if (save == null) return;

        inventory.Clear();

        foreach (var s in save.savedItems)
        {
            ItemData item = database.GetItemByID(s.itemId);
            if (item == null) continue;

            inventory.Add(new InventorySlot(
                item,
                s.count,
                item.maxDurability > 0 ? s.currentDurability : 1f
            ));
        }

        InitializeHotbarData();

        for (int i = 0; i < hotbarSize; i++)
        {
            hotbarData[i].Clear();

            if (i >= save.hotbarItems.Count) continue;

            var h = save.hotbarItems[i];

            if (h.itemId != -1)
            {
                ItemData item = database.GetItemByID(h.itemId);

                if (item != null)
                {
                    hotbarData[i].item = item;
                    hotbarData[i].count = h.count;
                    hotbarData[i].currentDurability =
                        item.maxDurability > 0 ? h.currentDurability : 1f;
                }
            }

            if (!string.IsNullOrEmpty(h.abilityName) && abilityDatabase != null)
                hotbarData[i].ability = abilityDatabase.GetAbilityByName(h.abilityName);
        }
    }

    public void RefreshAll()
    {
        SaveInventory();
        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();
        CraftUIManager.Instance?.UpdateInfoText();
    }

    public int GetTotalCount(ItemData data)
    {
        int total = 0;

        foreach (var s in inventory)
            if (s.item != null && s.item.itemID == data.itemID)
                total += s.count;

        foreach (var h in hotbarData)
            if (h.item != null && h.item.itemID == data.itemID)
                total += h.count;

        return total;
    }
}