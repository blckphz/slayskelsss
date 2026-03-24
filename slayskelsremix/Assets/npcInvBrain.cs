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
            Debug.LogError($"[NpcInv] {gameObject.name} tried to add null ItemData!");
            return;
        }

        Debug.Log($"[NpcInv] {gameObject.name} is attempting to add {amount}x {data.itemName}");

        // Check if we already have a stack of this item
        bool found = false;
        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                slot.count += amount;
                found = true;
                Debug.Log($"[NpcInv] {gameObject.name} increased stack of {data.itemName}. New count: {slot.count}");
                break;
            }
        }

        // Otherwise, add a new slot
        if (!found)
        {
            inventory.Add(new InventorySlot(data, amount));
            Debug.Log($"[NpcInv] {gameObject.name} added NEW item slot for {data.itemName}");
        }
    }
}