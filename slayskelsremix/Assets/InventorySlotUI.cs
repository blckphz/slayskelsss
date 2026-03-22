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
        currentCount = count; // 🔥 Crucial: Set the internal count

        if (item == null && ability == null) { ClearSlot(); return; }

        if (icon != null)
        {
            icon.sprite = item != null ? item.icon : ability.icon;
            icon.enabled = true;
        }

        if (amountText != null)
            amountText.text = (count > 1 && item != null) ? count.ToString() : "";
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
    public int GetCount() => currentCount; // 🔥 Used by Sync

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

    public void OnDrag(PointerEventData eventData) { if (dragPreviewIcon != null) dragPreviewIcon.transform.position = eventData.position; }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null) dragPreviewIcon.enabled = false;
        if (icon != null) icon.color = Color.white;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI dragged = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (dragged == null || dragged == this) return;

        // Check for stacking
        if (dragged.currentItem != null && dragged.currentItem == this.currentItem)
        {
            int newCount = this.currentCount + dragged.currentCount;
            this.SetSlot(this.currentItem, null, newCount);
            dragged.ClearSlot();
        }
        else
        {
            // SWAP EVERYTHING INCLUDING COUNT
            ItemData tempItem = dragged.currentItem;
            Ability tempAbility = dragged.currentAbility;
            int tempCount = dragged.currentCount;

            dragged.SetSlot(this.currentItem, this.currentAbility, this.currentCount);
            this.SetSlot(tempItem, tempAbility, tempCount);
        }

        // Tell the systems to save the new counts
        InvUI.Instance?.SyncToInventory();
        PlayerHotbarManager.Instance?.SyncHotbarToData();
    }
}