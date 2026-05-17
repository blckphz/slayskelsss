using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Elements")]
    public Image icon;
    public TextMeshProUGUI countText;
    public Slider durabilitySlider;
    public Image durabilityFill;
    public Image dragPreviewIcon;

    [Header("Runtime State")]
    [SerializeField] private ItemData currentItem;
    [SerializeField] private Ability currentAbility;
    [SerializeField] private int currentCount;
    [SerializeField] private float currentDurability;

    public bool IsEmpty => currentItem == null && currentAbility == null;

    public void SetSlot(ItemData item, Ability ability, int count, float durability)
    {
        currentItem = item;
        currentAbility = ability;
        currentCount = count;
        currentDurability = durability;

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

        UpdateSlotVisualElements();
    }

    public void SetSlot(ItemData item, Ability ability, int count)
    {
        float targetDurability = (item != null) ? item.maxDurability : 0f;
        SetSlot(item, ability, count, targetDurability);
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentAbility = null;
        currentCount = 0;
        currentDurability = 0f;

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (countText != null) countText.enabled = false;
        if (durabilitySlider != null) durabilitySlider.gameObject.SetActive(false);
    }

    private void UpdateSlotVisualElements()
    {
        if (countText != null)
        {
            if (currentCount > 1)
            {
                countText.text = currentCount.ToString();
                countText.enabled = true;
            }
            else
            {
                countText.enabled = false;
            }
        }

        if (durabilitySlider != null)
        {
            if (currentItem != null && currentItem.maxDurability > 0)
            {
                durabilitySlider.gameObject.SetActive(true);
                durabilitySlider.maxValue = currentItem.maxDurability;
                durabilitySlider.value = currentDurability;

                if (durabilityFill != null)
                {
                    float percentage = currentDurability / currentItem.maxDurability;
                    durabilityFill.color = Color.Lerp(Color.red, Color.green, percentage);
                }
            }
            else
            {
                durabilitySlider.gameObject.SetActive(false);
            }
        }
    }

    // ==========================================
    // DRAG AND DROP HANDLERS
    // ==========================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.sprite = icon.sprite;
            dragPreviewIcon.enabled = true;
            dragPreviewIcon.raycastTarget = false;
            dragPreviewIcon.transform.position = eventData.position;
        }

        if (icon != null) icon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null && dragPreviewIcon.enabled)
        {
            dragPreviewIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null) dragPreviewIcon.enabled = false;
        if (icon != null) icon.color = Color.white;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // -------------------------------------------------------------
        // CONDITION 1: DROPPING FROM CHEST TO PLAYER INVENTORY
        // -------------------------------------------------------------
        ChestSlotUI chestSlot = eventData.pointerDrag.GetComponent<ChestSlotUI>() ?? eventData.pointerDrag.GetComponentInParent<ChestSlotUI>();

        if (chestSlot != null)
        {
            if (chestSlot.GetItem() != null)
            {
                ItemData incomingItem = chestSlot.GetItem();
                int incomingCount = chestSlot.GetCount();
                float incomingDurability = chestSlot.GetDurability();

                ChestInventory currentChest = ChestUI.Instance != null ? ChestUI.Instance.GetCurrentChest() : null;

                if (currentChest != null)
                {
                    InventoryManager.Instance.AddItem(incomingItem, incomingCount, incomingDurability);
                    currentChest.RemoveItem(incomingItem, incomingCount);

                    chestSlot.ClearSlot();

                    ChestUI.Instance.Refresh();
                    if (InvUI.Instance != null) InvUI.Instance.RefreshUI();
                    if (PlayerHotbarManager.Instance != null) PlayerHotbarManager.Instance.RefreshHotbar();
                }
            }
            return; // Finished action sequence
        }

        // -------------------------------------------------------------
        // CONDITION 2: DROPPING FROM PLAYER SLOT TO ANOTHER PLAYER SLOT (SWAPPING)
        // -------------------------------------------------------------
        InventorySlotUI sourcePlayerSlot = eventData.pointerDrag.GetComponent<InventorySlotUI>() ?? eventData.pointerDrag.GetComponentInParent<InventorySlotUI>();

        if (sourcePlayerSlot != null && sourcePlayerSlot != this) // Don't drop on yourself!
        {
            // Cache our current destination data
            ItemData targetItem = this.currentItem;
            Ability targetAbility = this.currentAbility;
            int targetCount = this.currentCount;
            float targetDurability = this.currentDurability;

            // Cache incoming source data
            ItemData sourceItem = sourcePlayerSlot.GetItem();
            Ability sourceAbility = sourcePlayerSlot.GetAbility();
            int sourceCount = sourcePlayerSlot.GetCount();
            float sourceDurability = sourcePlayerSlot.GetDurability();

            // Perform the visual/state swap between the two slots
            this.SetSlot(sourceItem, sourceAbility, sourceCount, sourceDurability);
            sourcePlayerSlot.SetSlot(targetItem, targetAbility, targetCount, targetDurability);

            // Tell your UI components to save state changes back down to the model layer
            if (InvUI.Instance != null)
            {
                InvUI.Instance.SyncToInventory(); // Ensures underlying runtime list matches the new layout
                InvUI.Instance.RefreshUI();
            }
            if (PlayerHotbarManager.Instance != null)
            {
                PlayerHotbarManager.Instance.SyncHotbarToData();
                PlayerHotbarManager.Instance.RefreshHotbar();
            }
        }
    }

    // ==========================================
    // DATA LAYER GETTERS
    // ==========================================
    public ItemData GetItem() => currentItem;
    public Ability GetAbility() => currentAbility;
    public int GetCount() => currentCount;
    public float GetDurability() => currentDurability;
}