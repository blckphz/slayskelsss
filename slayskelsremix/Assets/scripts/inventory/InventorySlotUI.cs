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
    // CLICK INTERACTION (CAMPFIRE + CHEST RESTORED)
    // =========================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null || eventData.dragging) return;

        // =========================
        // 1. CAMPFIRE DEPOSIT
        // =========================
        if (CampfireUI.Instance != null && CampfireUI.Instance.CurrentCampfire != null)
        {
            CampfireBehav fire = CampfireUI.Instance.CurrentCampfire;

            if (fire.fuelItem == null || fire.fuelItem.itemID == currentItem.itemID)
            {
                int amount = 0;

                if (eventData.button == PointerEventData.InputButton.Left)
                    amount = 1;
                else if (eventData.button == PointerEventData.InputButton.Right)
                    amount = currentCount;

                int added = fire.AddFuel(currentItem, amount);

                if (added > 0)
                {
                    UpdateCount(-added);

                    if (!fire.isBurning)
                        fire.Ignite();

                    InvUI.Instance?.SyncToInventory();
                    PlayerHotbarManager.Instance?.SyncHotbarToData();
                    return;
                }
            }
        }

        // =========================
        // 2. CHEST INTERACTION
        // =========================
        ChestInventory chest = ChestUI.Instance != null ? ChestUI.Instance.GetCurrentChest() : null;

        if (chest != null)
        {
            int amountToMove = (eventData.button == PointerEventData.InputButton.Left)
                ? currentCount
                : 1;

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

    // =========================
    // COUNT UPDATE (🔥 FIXED SYNC)
    // =========================
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

        // 🔥 ALWAYS SYNC HOTBAR AFTER CHANGE
        PlayerHotbarManager.Instance?.SyncHotbarToData();
    }

    // =========================
    // UI
    // =========================
    private void RefreshUI()
    {
        if (amountText != null)
        {
            amountText.text =
                (currentItem != null && currentCount > 1)
                ? currentCount.ToString()
                : "";
        }

        if (icon != null)
        {
            icon.color = (currentItem != null || currentAbility != null)
                ? Color.white
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
            icon.color = new Color(1, 1, 1, 0);
        }

        if (amountText != null)
            amountText.text = "";
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

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.sprite = icon.sprite;
            dragPreviewIcon.enabled = true;
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
            icon.color = Color.white;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI dragged = eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        if (dragged != null && dragged != this)
        {
            ItemData tempItem = dragged.currentItem;
            Ability tempAbility = dragged.currentAbility;
            int tempCount = dragged.currentCount;

            dragged.SetSlot(currentItem, currentAbility, currentCount);
            SetSlot(tempItem, tempAbility, tempCount);

            InvUI.Instance?.SyncToInventory();
            PlayerHotbarManager.Instance?.SyncHotbarToData();
        }
    }
}