using System.Collections.Generic;
using UnityEngine;

public class NpcInvBrain : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;
        public InventorySlot(ItemData d, int c) { item = d; count = c; }
    }

    public List<InventorySlot> inventory = new List<InventorySlot>();

    public bool HasItem(int itemID)
    {
        return inventory.Exists(s => s.item != null && s.item.itemID == itemID && s.count > 0);
    }

    public void AddItem(ItemData data, int amount)
    {
        if (data == null) return;
        var slot = inventory.Find(s => s.item.itemID == data.itemID);
        if (slot != null) slot.count += amount;
        else inventory.Add(new InventorySlot(data, amount));

    }
}