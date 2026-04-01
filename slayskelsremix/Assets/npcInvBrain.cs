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

    [Header("NPC Storage")]
    // SerializeField ensures it shows up in the Unity Inspector
    [SerializeField] public List<InventorySlot> inventory = new List<InventorySlot>();

    public void AddItem(ItemData data, int amount)
    {
        if (data == null)
        {
            return;
        }


        // Check if we already have a stack of this item
        bool found = false;
        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                slot.count += amount;
                found = true;
                break;
            }
        }

        // Otherwise, add a new slot
        if (!found)
        {
            inventory.Add(new InventorySlot(data, amount));
        }
    }
}