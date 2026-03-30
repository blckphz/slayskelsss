using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChestSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;
    public Image dragPreviewIcon; // Assign the same global drag preview used in InventorySlotUI

    private ItemData currentItem;
    private int currentCount;

    public void SetSlot(ItemData item, int count)
    {
        currentItem = item;
        currentCount = count;

        if (item == null || count <= 0)
        {
            ClearSlot();
            return;
        }

        icon.sprite = item.icon;
        icon.enabled = true;
        amountText.text = count > 1 ? count.ToString() : "";
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentCount = 0;

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        if (amountText != null) amountText.text = "";
    }

    public ItemData GetItem() => currentItem;
    public int GetCount() => currentCount;

    // --- CLICK TO TRANSFER (Old Logic kept for convenience) ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
        {
            TransferToPlayer(currentCount);
        }
    }

    private void TransferToPlayer(int amount)
    {
        ChestInventory chest = ChestUI.Instance.GetCurrentChest();
        if (chest == null) return;

        // Try to add to player first
        InventoryManager.Instance.AddItem(currentItem, amount);
        // Remove from chest
        chest.RemoveItem(currentItem, amount);

        ChestUI.Instance.Refresh();
        InvUI.Instance.RefreshUI();
    }

    // --- DRAG AND DROP LOGIC ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.sprite = icon.sprite;
            dragPreviewIcon.enabled = true;
            dragPreviewIcon.raycastTarget = false;
        }
        if (icon != null) icon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null) dragPreviewIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null) dragPreviewIcon.enabled = false;
        if (icon != null) icon.color = Color.white;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Handle Item dropped FROM Player Inventory INTO Chest
        InventorySlotUI playerSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (playerSlot != null && playerSlot.GetItem() != null)
        {
            ItemData item = playerSlot.GetItem();
            int count = playerSlot.GetCount();

            ChestInventory chest = ChestUI.Instance.GetCurrentChest();
            if (chest != null)
            {
                // Add to chest
                if (chest.AddItem(item, count))
                {
                    // Remove from player
                    InventoryManager.Instance.RemoveItem(item, count);

                    ChestUI.Instance.Refresh();
                    InvUI.Instance.RefreshUI();
                }
            }
            return;
        }

        // Handle Item dropped FROM another Chest Slot (Swapping/Stacking)
        ChestSlotUI otherChestSlot = eventData.pointerDrag?.GetComponent<ChestSlotUI>();
        if (otherChestSlot != null && otherChestSlot != this)
        {
            // Logic for rearranging items inside the chest could go here
            // But usually, since chestItems is a List, Refresh() handles the order.
        }
    }
}