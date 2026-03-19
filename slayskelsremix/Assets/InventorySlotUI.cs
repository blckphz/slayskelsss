using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class InventorySlotUI : MonoBehaviour,
IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;

    private ItemData currentItem;
    private int currentCount;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 originalPosition;
    private Vector2 dragOffset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    // ---------------- DATA ----------------

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

        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }

    public ItemData GetItem() => currentItem;
    public int GetCount() => currentCount;

    // ---------------- DRAG ----------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        originalPosition = rectTransform.anchoredPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            eventData.position,
            Camera.main,
            out Vector2 localPointerPos
        );

        dragOffset = rectTransform.anchoredPosition - localPointerPos;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            eventData.position,
            Camera.main,
            out Vector2 localPointerPos
        );

        rectTransform.anchoredPosition = localPointerPos + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        rectTransform.anchoredPosition = originalPosition;
    }

    // ---------------- DROP ----------------

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        if (draggedSlot == null || draggedSlot == this) return;

        ItemData tempItem = draggedSlot.currentItem;
        int tempCount = draggedSlot.currentCount;

        draggedSlot.SetSlot(currentItem, currentCount);
        SetSlot(tempItem, tempCount);

        InvUI ui = GetComponentInParent<InvUI>();
        ui?.SyncToInventory();
    }

    // ---------------- CLICK ----------------

    public void OnPointerClick(PointerEventData eventData)
    {
        IItemUseHandler handler = GetComponentInParent<IItemUseHandler>();
        if (handler == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            handler.UseItem(currentItem);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            InvUI ui = GetComponentInParent<InvUI>();
            ui?.StartPlacingFromSlot(this);
        }
    }
}