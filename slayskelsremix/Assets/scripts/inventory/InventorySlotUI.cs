using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("UI Elements")]
    public Image icon;
    public TextMeshProUGUI amountText;
    public Image dragPreviewIcon;

    private ItemData currentItem;
    private Ability currentAbility;
    private int currentCount;

    // =========================
    // CLICK INTERACTION
    // =========================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null || eventData.dragging) return;

        // Campfire Deposit
        if (CampfireUI.Instance != null && CampfireUI.Instance.CurrentCampfire != null)
        {
            CampfireBehav fire = CampfireUI.Instance.CurrentCampfire;
            if (fire.fuelItem == null || fire.fuelItem.itemID == currentItem.itemID)
            {
                int amount = (eventData.button == PointerEventData.InputButton.Left) ? 1 : currentCount;
                int added = fire.AddFuel(currentItem, amount);
                if (added > 0)
                {
                    UpdateCount(-added);
                    if (!fire.isBurning) fire.Ignite();
                    InvUI.Instance?.SyncToInventory();
                    PlayerHotbarManager.Instance?.SyncHotbarToData();
                    return;
                }
            }
        }

        // Chest Interaction
        ChestInventory chest = ChestUI.Instance != null ? ChestUI.Instance.GetCurrentChest() : null;
        if (chest != null)
        {
            int amountToMove = (eventData.button == PointerEventData.InputButton.Left) ? currentCount : 1;
            int actual = Mathf.Min(amountToMove, currentCount);
            if (chest.AddItem(currentItem, actual))
            {
                UpdateCount(-actual);
                ChestUI.Instance.Refresh();
                InvUI.Instance?.SyncToInventory();
                PlayerHotbarManager.Instance?.SyncHotbarToData();
            }
        }
    }

    // =========================
    // SET SLOT
    // =========================
    public void SetSlot(ItemData item, Ability ability, int count)
    {
        currentItem = item;
        currentAbility = ability;
        currentCount = count;

        if (item == null && ability == null)
        {
            ClearSlot();
            return;
        }

        if (icon != null)
        {
            icon.sprite = item != null ? item.icon : ability.icon;
            icon.enabled = true;
            icon.color = Color.white;
        }
        RefreshUI();
    }

    public void UpdateCount(int amount)
    {
        currentCount += amount;
        if (currentCount <= 0) ClearSlot();
        else RefreshUI();
        PlayerHotbarManager.Instance?.SyncHotbarToData();
    }

    private void RefreshUI()
    {
        if (amountText != null)
        {
            amountText.text = (currentItem != null && currentCount > 1) ? currentCount.ToString() : "";
        }
        if (icon != null)
        {
            icon.color = (currentItem != null || currentAbility != null) ? Color.white : new Color(1, 1, 1, 0);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentAbility = null;
        currentCount = 0;
        if (icon != null) { icon.enabled = false; icon.color = new Color(1, 1, 1, 0); }
        if (amountText != null) amountText.text = "";
    }

    // =========================
    // GETTERS
    // =========================
    public ItemData GetItem() => currentItem;
    public Ability GetAbility() => currentAbility;
    public int GetCount() => currentCount;

    // =========================
    // DRAG SYSTEM
    // =========================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null && currentAbility == null) return;
        if (dragPreviewIcon != null) { dragPreviewIcon.sprite = icon.sprite; dragPreviewIcon.enabled = true; }
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
        InventorySlotUI dragged = eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        if (dragged != null && dragged != this)
        {
            ItemData draggedItem = dragged.GetItem();
            Ability draggedAbility = dragged.GetAbility();
            int draggedCount = dragged.GetCount();

            // Check if we should merge stacks
            bool isSameItem = draggedItem != null && currentItem != null && draggedItem.itemID == currentItem.itemID;

            if (isSameItem)
            {
                // MERGE: Add dragged count to this slot and clear the original
                UpdateCount(draggedCount);
                dragged.ClearSlot();
            }
            else
            {
                // SWAP: Standard exchange of data
                ItemData tempItem = currentItem;
                Ability tempAbility = currentAbility;
                int tempCount = currentCount;

                SetSlot(draggedItem, draggedAbility, draggedCount);
                dragged.SetSlot(tempItem, tempAbility, tempCount);
            }

            // Sync and Save changes
            InvUI.Instance?.SyncToInventory();
            PlayerHotbarManager.Instance?.SyncHotbarToData();
            InventoryManager.Instance?.SaveInventory();
        }
    }
}