using System.Collections.Generic;
using UnityEngine;

public class NpcInvBrain : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;
        public int currentDurability;

        public InventorySlot(ItemData d, int c)
        {
            item = d;
            count = c;

            if (d != null && d.usesDurability)
                currentDurability = d.maxDurability;
            else
                currentDurability = -1;
        }
    }

    [Header("Unique NPC Save ID")]
    public string npcId;

    [Header("Inventory")]
    public List<InventorySlot> inventory = new List<InventorySlot>();


    public UseableItem GetBestUsableItem(System.Predicate<UseableItem> match)
    {

        foreach (var slot in inventory)
        {
            if (slot.item is UseableItem u && slot.count > 0)
            {

                if (match(u))
                {
                    return u;
                }
            }
        }

        return null;
    }

    public UseableItem GetTool(ToolType tool)
    {

        foreach (var slot in inventory)
        {
            if (slot.item is UseableItem useable &&
                useable.abilityToExecute is ToolsSO toolSO)
            {

                if (toolSO.toolType == tool && slot.count > 0)
                {
                    return useable;
                }
            }
        }

        return null;
    }

    public bool HasItem(int itemID)
    {
        bool result = inventory.Exists(s =>
            s.item != null &&
            s.item.itemID == itemID &&
            s.count > 0);

        return result;
    }

    // =====================================================
    // ADD ITEM
    // =====================================================

    public void AddItem(ItemData data, int amount)
    {
        if (data == null)
        {
            return;
        }


        var slot = inventory.Find(s =>
            s.item != null &&
            s.item.itemID == data.itemID);

        if (slot != null)
        {
            slot.count += amount;
        }
        else
        {
            inventory.Add(new InventorySlot(data, amount));
        }
    }

    // =====================================================
    // CORE ITEM USAGE SYSTEM (FIXED + DEBUGGED)
    // =====================================================

    public bool TryUseItem(UseableItem item, Transform caster, Transform targetAnchor, GameObject target)
    {
        if (item == null)
            return false;

        var slot = inventory.Find(s => s.item == item && s.count > 0);

        if (slot == null)
        {
            Debug.Log($"[NPC INV] Item not found: {item.itemName}");
            return false;
        }

        Debug.Log($"[NPC INV] Using item: {item.itemName}");

        bool used = item.Use(caster, targetAnchor, target);

        if (!used)
        {
            Debug.LogWarning($"[NPC INV] Use FAILED: {item.itemName}");
            return false;
        }

        // ONLY consumables are reduced here
        if (item.itemType == ItemType.Consumable)
        {
            //slot.count--;

           // Debug.Log($"[NPC INV] Consumable used: {item.itemName} left={slot.count}");

            if (slot.count <= 0)
            {
               // inventory.Remove(slot);
              //  Debug.Log($"[NPC INV] Removed item: {item.itemName}");
            }
        }

        return true;
    }

    // =====================================================
    // SAVE SYSTEM
    // =====================================================

    public NpcInventorySaveData GetSaveData()
    {

        NpcInventorySaveData save = new NpcInventorySaveData();
        save.npcId = npcId;

        foreach (var slot in inventory)
        {
            if (slot.item == null)
                continue;

            save.items.Add(new SaveSlot
            {
                itemId = slot.item.itemID,
                count = slot.count
            });

        }

        return save;
    }

    public void LoadFromSave(NpcInventorySaveData save, ItemDatabase itemDatabase)
    {

        inventory.Clear();

        if (save == null)
        {
            return;
        }

        foreach (var s in save.items)
        {
            ItemData item = itemDatabase.GetItemByID(s.itemId);

            if (item == null)
            {
                continue;
            }

            inventory.Add(new InventorySlot(item, s.count));
        }

    }
}