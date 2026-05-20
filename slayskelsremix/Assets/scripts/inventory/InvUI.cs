using System.Collections.Generic;
using UnityEngine;

public class InvUI : MonoBehaviour
{
    public static InvUI Instance;

    [System.Serializable]
    public struct SlotUIData
    {
        [Tooltip("The actual Inventory Slot UI component.")]
        public InventorySlotUI slotUI;
    }

    [Header("References")]
    public InventoryManager inventoryManager;
    public BuildManager buildManager;

    [Header("Manual Slot UI Mapping")]
    [Tooltip("Manually assign each slot and its exact paired durability group here.")]
    public SlotUIData[] slotMappings;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    // ---------------- UI REFRESH ----------------
    public void RefreshUI()
    {
        if (inventoryManager == null || slotMappings == null) return;

        var inv = inventoryManager.inventory;

        for (int i = 0; i < slotMappings.Length; i++)
        {
            InventorySlotUI slot = slotMappings[i].slotUI;

            if (slot == null) continue;

            if (i < inv.Count)
            {
                ItemData currentItem = inv[i].item;

                // Update the slot data structures
                slot.SetSlot(currentItem, null, inv[i].count, inv[i].currentDurability);

                // Manually toggle the explicitly mapped group
            
            }
            else
            {
                slot.ClearSlot();
            
            }
        }
    }

    // ---------------- SYNC ----------------
    public void SyncToInventory()
    {
        if (inventoryManager == null || slotMappings == null) return;

        inventoryManager.inventory.Clear();

        foreach (var mapping in slotMappings)
        {
            if (mapping.slotUI == null) continue;

            ItemData item = mapping.slotUI.GetItem();
            int count = mapping.slotUI.GetCount();
            float durability = mapping.slotUI.GetDurability();

            if (item != null && count > 0)
            {
                inventoryManager.inventory.Add(
                    new InventoryManager.InventorySlot(item, count, durability)
                );
            }
            else
            {
                inventoryManager.inventory.Add(
                    new InventoryManager.InventorySlot(null, 0, 0f)
                );
            }
        }

        inventoryManager.SaveInventory();
    }

    // ---------------- USE ITEM ----------------
    public void UseItem(ItemData item)
    {
        if (item == null) return;

        switch (item.itemType)
        {
            case ItemType.Consumable:
                UseConsumable(item);
                break;

            case ItemType.Placeable:
                if (buildManager != null)
                {
                    buildSO buildItem = item as buildSO;
                    if (buildItem != null)
                    {
                        buildManager.StartPlacing(buildItem);
                    }
                }
                break;
        }
    }

    public void UseConsumable(ItemData item)
    {
        Debug.Log($"[InvUI] Consumed {item.itemName}");
        bool wasRemoved = inventoryManager.RemoveItem(item, 1);

        if (wasRemoved)
        {
            SyncToInventory();
            RefreshUI();
        }
    }

    public void StartPlacingFromSlot(InventorySlotUI slot)
    {
        if (slot == null) return;
        ItemData item = slot.GetItem();
        if (item == null) return;

        buildSO buildItem = item as buildSO;
        if (buildItem != null && buildManager != null)
        {
            buildManager.StartPlacing(buildItem);
        }
    }
}