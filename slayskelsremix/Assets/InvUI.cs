using UnityEngine;
using System.Collections.Generic;

public class InvUI : MonoBehaviour
{
    public static InvUI Instance;

    [Header("References")]
    public InventoryManager inventoryManager;
    public InventorySlotUI[] slots;
    public BuildManager buildManager;

    private void Awake()
    {
        // Singleton pattern to ensure only one InvUI exists
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
        // Initial fill of the UI on game start
        RefreshUI();
    }

    // ---------------- UI REFRESH ----------------
    // Call this when the underlying data changes via script (e.g., picking up an item)
    public void RefreshUI()
    {
        if (inventoryManager == null) return;

        var inv = inventoryManager.inventory;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inv.Count)
            {
                // Set the UI slot to match the data list
                // Main inventory usually doesn't show abilities, so we pass null
                slots[i].SetSlot(inv[i].item, null, inv[i].count);
            }
            else
            {
                // If there's no data for this index, make sure the slot looks empty
                slots[i].ClearSlot();
            }
        }
    }

    // ---------------- SYNC ----------------
    // Call this after a manual UI change (like Drag & Drop) to update the data
    public void SyncToInventory()
    {
        if (inventoryManager == null) return;

        // 1. Wipe the current data list in the manager
        inventoryManager.inventory.Clear();

        // 2. Rebuild the list based EXACTLY on what the user positioned in the UI
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

        // 3. Save the new state to the JSON file
        inventoryManager.SaveInventory();

        // NOTE: We do NOT call RefreshUI() here to prevent "Snap Back" glitches.
        // The slots already visually represent the correct state after the Drop.
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

    private void UseConsumable(ItemData item)
    {
        Debug.Log($"[InvUI] Consumed {item.itemName}");

        // Remove the item from the data logic
        bool wasRemoved = inventoryManager.RemoveItem(item, 1);

        // If an item was successfully removed via code, we must refresh the visuals
        if (wasRemoved)
        {
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