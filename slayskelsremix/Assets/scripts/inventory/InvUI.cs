using UnityEngine;

public class InvUI : MonoBehaviour
{
    public static InvUI Instance;

    [System.Serializable]
    public struct SlotUIData { public InventorySlotUI slotUI; }

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
            var slot = slotMappings[i].slotUI;
            if (slot == null) continue;

            if (i < inv.Count)
            {
                var item = inv[i].item;
                Debug.Log($"[InvUI] Slot {i} set to: {(item != null ? item.itemName : "Null")} | Qty: {inv[i].count}");
                slot.SetSlot(item, null, inv[i].count, inv[i].currentDurability);
            }
            else
            {
                slot.ClearSlot();
            }
        }
    }

    public void SyncToInventory()
    {
        if (inventoryManager == null) return;
        inventoryManager.inventory.Clear();

        foreach (var mapping in slotMappings)
        {
            if (mapping.slotUI == null) continue;
            var item = mapping.slotUI.GetItem();
            if (item != null)
            {
                inventoryManager.inventory.Add(new InventoryManager.InventorySlot(item, mapping.slotUI.GetCount(), mapping.slotUI.GetDurability()));
            }
        }
        inventoryManager.SaveInventory();
    }
}