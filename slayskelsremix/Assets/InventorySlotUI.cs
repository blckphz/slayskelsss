using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Elements")]
    public Image icon;
    public TextMeshProUGUI amountText;
    public Image dragPreviewIcon;

    private ItemData currentItem;
    private Ability currentAbility;
    private int currentCount;

    public void SetSlot(ItemData item, Ability ability, int count)
    {
        currentItem = item;
        currentAbility = ability;
        currentCount = count;

        if (item == null && ability == null) { ClearSlot(); return; }

        if (icon != null)
        {
            icon.sprite = item != null ? item.icon : ability.icon;
            icon.enabled = true;
        }

        RefreshUI();
    }

    /// <summary>
    /// 🔥 NEW: Updates the stack count and refreshes the text.
    /// Used by BuildManager via PlayerHotbarManager.
    /// </summary>
    public void UpdateCount(int amount)
    {
        currentCount += amount;

        if (currentCount <= 0)
        {
            ClearSlot();
        }
        else
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (amountText != null)
        {
            // Only show count if it's an item (not an ability) and stack is > 1
            amountText.text = (currentCount > 1 && currentItem != null) ? currentCount.ToString() : "";
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentAbility = null;
        currentCount = 0;
        if (icon != null) icon.enabled = false;
        if (amountText != null) amountText.text = "";
    }

    public ItemData GetItem() => currentItem;
    public Ability GetAbility() => currentAbility;
    public int GetCount() => currentCount;

    // ---------------- DRAG & DROP LOGIC ----------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null && currentAbility == null) return;
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
        // 1. Check if dropped from another Player Inventory Slot
        InventorySlotUI draggedPlayerSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedPlayerSlot != null && draggedPlayerSlot != this)
        {
            HandlePlayerToPlayerSwap(draggedPlayerSlot);
            return;
        }

        // 2. 🔥 NEW: Check if dropped from a CHEST Slot
        ChestSlotUI draggedChestSlot = eventData.pointerDrag?.GetComponent<ChestSlotUI>();
        if (draggedChestSlot != null)
        {
            ItemData item = draggedChestSlot.GetItem();
            int count = draggedChestSlot.GetCount();

            ChestInventory chest = ChestUI.Instance.GetCurrentChest();
            if (chest != null && item != null)
            {
                // Remove from chest
                chest.RemoveItem(item, count);
                // Add to player (InventoryManager handles finding a stack or new slot)
                InventoryManager.Instance.AddItem(item, count);

                ChestUI.Instance.Refresh();
                InvUI.Instance.RefreshUI();
            }
        }
    }

    private void HandlePlayerToPlayerSwap(InventorySlotUI dragged)
    {
        if (dragged.currentItem != null && dragged.currentItem == this.currentItem)
        {
            int newCount = this.currentCount + dragged.currentCount;
            this.SetSlot(this.currentItem, null, newCount);
            dragged.ClearSlot();
        }
        else
        {
            ItemData tempItem = dragged.currentItem;
            Ability tempAbility = dragged.currentAbility;
            int tempCount = dragged.currentCount;

            dragged.SetSlot(this.currentItem, this.currentAbility, this.currentCount);
            this.SetSlot(tempItem, tempAbility, tempCount);
        }

        InvUI.Instance?.SyncToInventory();
        PlayerHotbarManager.Instance?.SyncHotbarToData();
    }
}