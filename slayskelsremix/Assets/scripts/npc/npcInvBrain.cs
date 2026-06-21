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

    // =====================================================
    // DEBUG HELPERS
    // =====================================================

    private void Log(string msg)
    {
        Debug.Log($"<color=cyan>[NPC INV]</color> [{name}] {msg}");
    }

    private void Warn(string msg)
    {
        Debug.LogWarning($"<color=yellow>[NPC INV]</color> [{name}] {msg}");
    }

    private void Error(string msg)
    {
        Debug.LogError($"<color=red>[NPC INV]</color> [{name}] {msg}");
    }

    // =====================================================
    // ITEM SEARCH
    // =====================================================

    public UseableItem GetBestUsableItem(System.Predicate<UseableItem> match)
    {
        Log("Searching for usable item...");

        foreach (var slot in inventory)
        {
            if (slot.item is UseableItem u && slot.count > 0)
            {
                Log($"Checking item: {u.itemName}");

                if (match(u))
                {
                    Log($"FOUND MATCH: {u.itemName}");
                    return u;
                }
            }
        }

        Warn("No matching usable item found.");
        return null;
    }

    public UseableItem GetTool(ToolType tool)
    {
        Log($"Searching tool: {tool}");

        foreach (var slot in inventory)
        {
            if (slot.item is UseableItem useable &&
                useable.abilityToExecute is ToolsSO toolSO)
            {
                Log($"Checking tool item: {useable.itemName} -> {toolSO.toolType}");

                if (toolSO.toolType == tool && slot.count > 0)
                {
                    Log($"FOUND TOOL: {useable.itemName}");
                    return useable;
                }
            }
        }

        Warn("No tool found.");
        return null;
    }

    public bool HasItem(int itemID)
    {
        bool result = inventory.Exists(s =>
            s.item != null &&
            s.item.itemID == itemID &&
            s.count > 0);

        Log($"HasItem({itemID}) = {result}");
        return result;
    }

    // =====================================================
    // ADD ITEM
    // =====================================================

    public void AddItem(ItemData data, int amount)
    {
        if (data == null)
        {
            Error("Attempted to add NULL item!");
            return;
        }

        Log($"Adding item: {data.name} x{amount}");

        var slot = inventory.Find(s =>
            s.item != null &&
            s.item.itemID == data.itemID);

        if (slot != null)
        {
            slot.count += amount;
            Log($"Stack updated -> {data.name} total: {slot.count}");
        }
        else
        {
            inventory.Add(new InventorySlot(data, amount));
            Log($"New item added -> {data.name}");
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
        Log("Saving inventory...");

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

            Log($"Saved: {slot.item.name} x{slot.count}");
        }

        Log($"Save complete. Items: {save.items.Count}");
        return save;
    }

    public void LoadFromSave(NpcInventorySaveData save, ItemDatabase itemDatabase)
    {
        Log("Loading inventory...");

        inventory.Clear();

        if (save == null)
        {
            Warn("Save is NULL.");
            return;
        }

        foreach (var s in save.items)
        {
            ItemData item = itemDatabase.GetItemByID(s.itemId);

            if (item == null)
            {
                Error($"Missing item ID: {s.itemId}");
                continue;
            }

            inventory.Add(new InventorySlot(item, s.count));
            Log($"Loaded: {item.name} x{s.count}");
        }

        Log($"Load complete. Total items: {inventory.Count}");
    }
}