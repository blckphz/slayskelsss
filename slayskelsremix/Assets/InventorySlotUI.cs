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

    // ---------------- CLICK (TRANSFER LOGIC) ----------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // Check if a chest is currently open in the UI
        ChestInventory openChest = (ChestUI.Instance != null) ? ChestUI.Instance.GetCurrentChest() : null;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (openChest != null)
            {
                // CASE A: Chest is open -> Transfer 1 item to the chest
                TransferToChest(openChest);
            }
            else
            {
                // CASE B: No chest open -> Use the item (Consume/Place)
                IItemUseHandler handler = GetComponentInParent<IItemUseHandler>();
                handler?.UseItem(currentItem);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click: Always try to start building/placing
            InvUI ui = GetComponentInParent<InvUI>();
            ui?.StartPlacingFromSlot(this);
        }
    }

    private void TransferToChest(ChestInventory chest)
    {
        Debug.Log($"[InventorySlotUI] Transferring {currentItem.itemName} to Chest");

        // 1. Add item to chest data
        bool addedToChest = chest.AddItem(currentItem, 1);

        if (addedToChest)
        {
            // 2. Remove item from player inventory data
            // This also triggers InvUI.Instance.RefreshUI() automatically based on our previous logic
            InventoryManager.Instance.RemoveItem(currentItem, 1);

            // 3. Manually refresh the Chest UI so the item appears there immediately
            ChestUI.Instance.Refresh();
        }
    }
}