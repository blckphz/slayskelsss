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

    // ---------------------------------------------------
    // INTERACTION: CLICK TO DEPOSIT (CAMPFIRE OR CHEST)
    // ---------------------------------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null || eventData.dragging) return;

        // --- 1. HANDLE CAMPFIRE INTERACTION ---
        if (CampfireUI.Instance != null && CampfireUI.Instance.CurrentCampfire != null)
        {
            CampfireBehav activeFire = CampfireUI.Instance.CurrentCampfire;

            // Check if item is valid fuel
            if (activeFire.fuelItem == null || currentItem.itemID == activeFire.fuelItem.itemID)
            {
                int amountToDeposit = 0;

                // LEFT CLICK: Deposit 1 (Standard for Campfire)
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    amountToDeposit = 1;
                }
                // RIGHT CLICK: Deposit All (Standard for Campfire)
                else if (eventData.button == PointerEventData.InputButton.Right)
                {
                    amountToDeposit = currentCount;
                }

                if (amountToDeposit > 0)
                {
                    int added = activeFire.AddFuel(currentItem, amountToDeposit);

                    if (added > 0)
                    {
                        UpdateCount(-added);
                        if (!activeFire.isBurning) activeFire.Ignite();
                        InvUI.Instance?.SyncToInventory();
                        return; // Exit so we don't trigger chest logic
                    }
                }
            }
        }

        // --- 2. HANDLE CHEST INTERACTION ---
        ChestInventory openChest = ChestUI.Instance != null ? ChestUI.Instance.GetCurrentChest() : null;
        if (openChest != null)
        {
            int amountToMove = 0;

            // LEFT CLICK: Transfer All
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                amountToMove = currentCount;
            }
            // RIGHT CLICK: Transfer 1
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                amountToMove = 1;
            }

            if (amountToMove > 0)
            {
                int actualToMove = Mathf.Min(amountToMove, currentCount);

                if (openChest.AddItem(currentItem, actualToMove))
                {
                    UpdateCount(-actualToMove);
                    ChestUI.Instance.Refresh();
                    InvUI.Instance?.SyncToInventory();
                }
            }
        }
    }

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
            icon.color = new Color(1, 1, 1, 1); // Opacity 100%
        }

        RefreshUI();
    }

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
            amountText.text =
                (currentItem != null && currentCount > 1)
                ? currentCount.ToString()
                : "";
        }

        // Safety check for opacity
        if (icon != null)
        {
            icon.color = (currentItem != null || currentAbility != null)
                ? new Color(1, 1, 1, 1)
                : new Color(1, 1, 1, 0);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentAbility = null;
        currentCount = 0;

        if (icon != null)
        {
            icon.enabled = false;
            icon.color = new Color(1, 1, 1, 0); // Opacity 0%
        }

        if (amountText != null)
            amountText.text = "";
    }

    public ItemData GetItem() => currentItem;
    public Ability GetAbility() => currentAbility;
    public int GetCount() => currentCount;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null && currentAbility == null)
            return;

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.sprite = icon.sprite;
            dragPreviewIcon.enabled = true;
            dragPreviewIcon.raycastTarget = false;
        }

        if (icon != null)
            icon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null)
            dragPreviewIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null)
            dragPreviewIcon.enabled = false;

        if (icon != null)
            icon.color = currentItem != null ? Color.white : new Color(1, 1, 1, 0);
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedPlayerSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedPlayerSlot != null && draggedPlayerSlot != this)
        {
            HandlePlayerToPlayerSwap(draggedPlayerSlot);
            return;
        }

        ChestSlotUI draggedChestSlot = eventData.pointerDrag?.GetComponent<ChestSlotUI>();
        if (draggedChestSlot != null)
        {
            ItemData item = draggedChestSlot.GetItem();
            int count = draggedChestSlot.GetCount();
            ChestInventory chest = ChestUI.Instance.GetCurrentChest();

            if (chest != null && item != null)
            {
                // Remove from chest first
                chest.RemoveItem(item, count);
                // Then add to player manager (Standard logic)
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