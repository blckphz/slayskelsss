using UnityEngine;

public class InvUI : MonoBehaviour
{
    public static InvUI Instance;

    [System.Serializable]
    public struct SlotUIData
    {
        public InventorySlotUI slotUI;
    }

    public InventoryManager inventoryManager;
    public SlotUIData[] slotMappings;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() => RefreshUI();

    public void RefreshUI()
    {
        if (inventoryManager == null) return;

        var inv = inventoryManager.inventory;

        for (int i = 0; i < slotMappings.Length; i++)
        {
            var slotUI = slotMappings[i].slotUI;
            if (slotUI == null) continue;

            // NEW: Check if this UI slot is actually part of the hotbar
            // If it is, we should be careful not to overwrite it with 
            // stale data from inventoryManager.inventory
            if (PlayerHotbarManager.Instance.hotbarSlots.Contains(slotUI))
                continue;

            if (i < inv.Count && inv[i].item != null)
            {
               slotUI.SetSlot(inv[i].item, null, inv[i].count, (int)inv[i].currentDurability);
            }
            else
            {
                slotUI.ClearSlot();
            }
        }
    }

    public void SyncToInventory()
    {
        if (inventoryManager == null) return;

        for (int i = 0; i < slotMappings.Length; i++)
        {
            var slotUI = slotMappings[i].slotUI;
            if (slotUI == null || i >= inventoryManager.inventory.Count) continue;

            var slot = inventoryManager.inventory[i];

            slot.item = slotUI.GetItem();
            slot.count = slotUI.GetCount();
            slot.currentDurability = slotUI.GetDurability();
        }

        inventoryManager.SaveInventory();
    }
}