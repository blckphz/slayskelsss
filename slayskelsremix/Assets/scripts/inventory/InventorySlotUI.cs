using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
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
    private int SlotIndex => transform.GetSiblingIndex();

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

    public void UpdateSlotVisualElements()
    {
        if (countText != null)
        {
            countText.text = currentCount.ToString();
            countText.enabled = (currentCount > 1);
        }

        if (currentItem != null && !currentItem.IsUnbreakable)
        {
            if (durabilitySlider != null)
            {
                durabilitySlider.gameObject.SetActive(true);
                durabilitySlider.maxValue = currentItem.maxDurability;
                durabilitySlider.value = currentDurability;
            }

            if (durabilityFill != null)
            {
                float percentage = currentItem.maxDurability > 0 ? (currentDurability / currentItem.maxDurability) : 0f;
                durabilityFill.color = Color.Lerp(Color.red, Color.green, percentage);
            }
        }
        else
        {
            if (durabilitySlider != null) durabilitySlider.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsEmpty || eventData.dragging) return;

        if (invToolTip.Instance != null && currentItem != null)
        {
            invToolTip.Instance.ShowToolTip(currentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (invToolTip.Instance != null)
        {
            invToolTip.Instance.HideToolTip();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        if (invToolTip.Instance != null) invToolTip.Instance.HideToolTip();

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

        // 1. CHEST SOURCE HANDLING
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
                    TriggerGlobalUIAndDataRefresh();
                }
            }
            return;
        }

        // 2. PLAYER INVENTORY SOURCE HANDLING
        InventorySlotUI sourcePlayerSlot = eventData.pointerDrag.GetComponent<InventorySlotUI>() ?? eventData.pointerDrag.GetComponentInParent<InventorySlotUI>();

        if (sourcePlayerSlot != null && sourcePlayerSlot != this)
        {
            ItemData targetItem = this.currentItem;
            Ability targetAbility = this.currentAbility;
            int targetCount = this.currentCount;
            float targetDurability = this.currentDurability;

            ItemData sourceItem = sourcePlayerSlot.GetItem();
            Ability sourceAbility = sourcePlayerSlot.GetAbility();
            int sourceCount = sourcePlayerSlot.GetCount();
            float sourceDurability = sourcePlayerSlot.GetDurability();

            // ✅ STACK BLENDING LOGIC
            if (targetItem != null && sourceItem != null && targetItem.itemID == sourceItem.itemID && targetItem.IsUnbreakable)
            {
                int maxStack = targetItem.maxStackSize;
                int spaceLeft = maxStack - targetCount;

                if (spaceLeft > 0)
                {
                    int amountToMove = Mathf.Min(spaceLeft, sourceCount);
                    
                    // Add quantities to this destination slot
                    this.SetSlot(targetItem, targetAbility, targetCount + amountToMove, targetDurability);

                    // Compute remnants left over on source slot
                    int remnants = sourceCount - amountToMove;
                    if (remnants > 0)
                    {
                        sourcePlayerSlot.SetSlot(sourceItem, sourceAbility, remnants, sourceDurability);
                    }
                    else
                    {
                        sourcePlayerSlot.ClearSlot();
                    }

                    FinalizeStackTransaction(sourcePlayerSlot);
                    return;
                }
            }

            // Standard Swap Fallback if items are mismatched, full, or have active tool durability loops
            this.SetSlot(sourceItem, sourceAbility, sourceCount, sourceDurability);
            sourcePlayerSlot.SetSlot(targetItem, targetAbility, targetCount, targetDurability);

            FinalizeStackTransaction(sourcePlayerSlot);
        }
    }

    private void FinalizeStackTransaction(InventorySlotUI sourcePlayerSlot)
    {
        TriggerGlobalUIAndDataRefresh();

        this.UpdateSlotVisualElements();
        sourcePlayerSlot.UpdateSlotVisualElements();

        if (invToolTip.Instance != null)
        {
            invToolTip.Instance.HideToolTip();
            if (!IsEmpty) invToolTip.Instance.ShowToolTip(currentItem);
        }
    }

    private void TriggerGlobalUIAndDataRefresh()
    {
        if (InvUI.Instance != null) InvUI.Instance.SyncToInventory();
        if (PlayerHotbarManager.Instance != null) PlayerHotbarManager.Instance.SyncHotbarToData();

        if (InvUI.Instance != null) InvUI.Instance.RefreshUI();
        if (PlayerHotbarManager.Instance != null) PlayerHotbarManager.Instance.RefreshHotbar();
    }

    public ItemData GetItem() => currentItem;
    public Ability GetAbility() => currentAbility;
    public int GetCount() => currentCount;
    public float GetDurability() => currentDurability;
}