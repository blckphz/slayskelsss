using UnityEngine;

public class InvUI : MonoBehaviour, IItemUseHandler
{
    [Header("References")]
    public InventoryManager inventoryManager;
    public InventorySlotUI[] slots;
    public BuildManager buildManager;

    private void Start()
    {
        RefreshUI();
    }

    // ---------------- UI REFRESH ----------------

    public void RefreshUI()
    {
        var inv = inventoryManager.inventory;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inv.Count)
            {
                slots[i].SetSlot(inv[i].item, inv[i].count);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    // ---------------- SYNC ----------------

    public void SyncToInventory()
    {
        inventoryManager.inventory.Clear();

        foreach (var slot in slots)
        {
            ItemData item = slot.GetItem();
            int count = slot.GetCount();

            if (item != null && count > 0)
            {
                inventoryManager.inventory.Add(
                    new InventoryManager.InventorySlot(item, count)
                );
            }
        }

        inventoryManager.SaveInventory();
    }

    // ---------------- INTERFACE IMPLEMENTATION ----------------

    public void UseItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("[InvUI] Tried to use null item.");
            return;
        }

        Debug.Log("[InvUI] Using item: " + item.itemName);

        switch (item.itemType)
        {
            case ItemType.Consumable:
                UseConsumable(item);
                break;

            case ItemType.Placeable:
                if (buildManager != null)
                {
                    buildManager.StartPlacing(item);
                }
                else
                {
                    Debug.LogError("[InvUI] BuildManager is not assigned!");
                }
                break;
        }
    }

    private void UseConsumable(ItemData item)
    {
        Debug.Log($"[InvUI] Consumed {item.itemName} (+{item.healAmount})");

        // Example hook:
        // playerHealth.Heal(item.healAmount);
    }

    // ---------------- BUILDING FROM SLOT ----------------

    public void StartPlacingFromSlot(InventorySlotUI slot)
    {
        if (slot == null)
        {
            Debug.LogWarning("[InvUI] StartPlacingFromSlot called with null slot.");
            return;
        }

        ItemData item = slot.GetItem();

        if (item == null)
        {
            Debug.LogWarning("[InvUI] Slot has no item.");
            return;
        }

        if (item.placeablePrefab == null)
        {
            Debug.LogWarning("[InvUI] Item is not placeable: " + item.itemName);
            return;
        }

        Debug.Log("[InvUI] Starting placement from slot: " + item.itemName);

        if (buildManager != null)
        {
            buildManager.StartPlacing(item);
        }
        else
        {
            Debug.LogError("[InvUI] BuildManager not assigned!");
        }
    }
}