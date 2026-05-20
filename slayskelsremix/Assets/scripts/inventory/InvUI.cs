using UnityEngine;

public class InvUI : MonoBehaviour
{
    public static InvUI Instance;

    public InventoryManager inventoryManager;
    public InventorySlotUI[] slots;

    private void Awake()
    {
        Instance = this;
    }

    public void RefreshUI()
    {
        Debug.Log("[INV REFRESH] Updating UI from data");

        for (int i = 0; i < slots.Length; i++)
        {
            if (i >= inventoryManager.inventory.Count) continue;

            var data = inventoryManager.inventory[i];

            Debug.Log($"[LOAD SLOT {i}] item={data.item?.name ?? "NULL"} x{data.count}");

            if (data.item != null && data.count > 0)
                slots[i].SetSlot(data.item, null, data.count, data.currentDurability);
            else
                slots[i].ClearSlot();
        }
    }

    public void SyncToInventory()
    {
        Debug.Log($"[SAVE START] Saving {slots.Length} slots...");

        for (int i = 0; i < slots.Length; i++)
        {
            if (i >= inventoryManager.inventory.Count) continue;

            var inv = inventoryManager.inventory[i];
            var ui = slots[i];

            inv.item = ui.GetItem();
            inv.count = ui.GetCount();
            inv.currentDurability = ui.GetDurability();

            Debug.Log($"[SAVE SLOT {i}] item={inv.item?.name ?? "NULL"} x{inv.count} dur={inv.currentDurability}");
        }

        inventoryManager.SaveInventory();
        Debug.Log("[SAVE COMPLETE]");
    }
}