using System.Collections.Generic;
using UnityEngine;

public class NpcInvBrain : MonoBehaviour
{
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

    [Header("Unique NPC Save ID")]
    public string npcId;

    public List<InventorySlot> inventory = new List<InventorySlot>();

    public bool HasItem(int itemID)
    {
        return inventory.Exists(s =>
            s.item != null &&
            s.item.itemID == itemID &&
            s.count > 0);
    }

    public void AddItem(ItemData data, int amount)
    {
        if (data == null)
        {
            Debug.LogWarning($"{name}: Tried to add NULL item.");
            return;
        }

        var slot = inventory.Find(s =>
            s.item != null &&
            s.item.itemID == data.itemID);

        if (slot != null)
        {
            slot.count += amount;
            Debug.Log($"{name}: Added {amount}x {data.name}. New count = {slot.count}");
        }
        else
        {
            inventory.Add(new InventorySlot(data, amount));
            Debug.Log($"{name}: Added NEW item {data.name} x{amount}");
        }
    }

    public NpcInventorySaveData GetSaveData()
    {
        Debug.Log($"=== SAVING NPC: {name} | npcId={npcId} ===");

        NpcInventorySaveData save = new NpcInventorySaveData();
        save.npcId = npcId;

        foreach (var slot in inventory)
        {
            if (slot.item == null)
            {
                Debug.LogWarning($"{name}: Found NULL item in inventory.");
                continue;
            }

            Debug.Log($"Saving item: {slot.item.name} (ID={slot.item.itemID}) x{slot.count}");

            save.items.Add(new SaveSlot
            {
                itemId = slot.item.itemID,
                count = slot.count
            });
        }

        Debug.Log($"Saved {save.items.Count} items for {name}");
        return save;
    }

    public void LoadFromSave(NpcInventorySaveData save, ItemDatabase itemDatabase)
    {
        Debug.Log($"=== LOADING NPC: {name} | npcId={npcId} ===");

        inventory.Clear();

        if (save == null)
        {
            Debug.LogWarning($"{name}: save data is NULL.");
            return;
        }

        if (save.items.Count == 0)
        {
            Debug.LogWarning($"{name}: save has 0 items.");
        }

        foreach (var savedSlot in save.items)
        {
            Debug.Log($"Trying to load item ID={savedSlot.itemId} x{savedSlot.count}");

            ItemData item = itemDatabase.GetItemByID(savedSlot.itemId);

            if (item == null)
            {
                Debug.LogError($"{name}: Could NOT find ItemData for ID {savedSlot.itemId} in ItemDatabase!");
                continue;
            }

            inventory.Add(new InventorySlot(item, savedSlot.count));

            Debug.Log($"Loaded: {item.name} x{savedSlot.count}");
        }

        Debug.Log($"{name}: Finished loading. Inventory count = {inventory.Count}");
    }
}