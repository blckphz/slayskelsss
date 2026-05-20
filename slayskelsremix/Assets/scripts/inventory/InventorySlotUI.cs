using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
   
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI countText;
    public Slider durabilitySlider;
    public Image dragPreviewIcon;

    [Header("State")]
    [SerializeField] private ItemData currentItem;
    [SerializeField] private Ability currentAbility;
    [SerializeField] private int currentCount;
    [SerializeField] private float currentDurability;

    public bool IsEmpty => currentItem == null && currentAbility == null;
    private int SlotIndex => transform.GetSiblingIndex();

    // =========================
    // SET SLOT
    // =========================
    public void SetSlot(ItemData item, Ability ability, int count, float durability)
    {
        Debug.Log($"[SLOT SET] Slot {SlotIndex} <- {(item ? item.name : "NULL")} x{count} dur={durability}");

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
            icon.enabled = true;
            icon.sprite = item != null ? item.icon : ability.icon;
        }

        UpdateVisuals();
    }

    // =========================
    // CLEAR SLOT
    // =========================
    public void ClearSlot()
    {
        Debug.Log($"[SLOT CLEAR] Slot {SlotIndex}");

        currentItem = null;
        currentAbility = null;
        currentCount = 0;
        currentDurability = 0;

        if (icon != null)
            icon.enabled = false;

        if (countText != null)
            countText.text = "";

        if (durabilitySlider != null)
            durabilitySlider.gameObject.SetActive(false);
    }

    // =========================
    // VISUALS
    // =========================
    private void UpdateVisuals()
    {
        if (countText != null)
            countText.text = currentCount > 1 ? currentCount.ToString() : "";

        if (durabilitySlider != null)
        {
            if (currentItem != null && !currentItem.IsUnbreakable)
            {
                durabilitySlider.gameObject.SetActive(true);
                durabilitySlider.maxValue = currentItem.maxDurability;
                durabilitySlider.value = currentDurability;
            }
            else
            {
                durabilitySlider.gameObject.SetActive(false);
            }
        }
    }

    // =========================
    // DRAG
    // =========================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        Debug.Log($"[DRAG START] Slot {SlotIndex}");

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.sprite = icon.sprite;
            dragPreviewIcon.enabled = true;
        }

        icon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null && dragPreviewIcon.enabled)
            dragPreviewIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null)
            dragPreviewIcon.enabled = false;

        icon.color = Color.white;
    }

    // =========================
    // DROP
    // =========================
    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI source =
            eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        if (source == null || source == this) return;

        Debug.Log($"[SWAP PREVIEW] Slot {source.SlotIndex} -> Slot {SlotIndex}");

        // capture snapshot BEFORE changes
        var srcItem = source.currentItem;
        var srcCount = source.currentCount;
        var srcDur = source.currentDurability;

        var tgtItem = currentItem;
        var tgtCount = currentCount;
        var tgtDur = currentDurability;

        Debug.Log($"[SWAP EXECUTE] {source.SlotIndex} ↔ {SlotIndex}");

        // swap
        source.SetSlot(tgtItem, null, tgtCount, tgtDur);
        SetSlot(srcItem, null, srcCount, srcDur);

        Debug.Log($"[SWAP RESULT] Slot {source.SlotIndex}={source.currentItem?.name ?? "EMPTY"} | Slot {SlotIndex}={currentItem?.name ?? "EMPTY"}");

        FinalizeTransaction(source);
    }

    private void FinalizeTransaction(InventorySlotUI source)
    {
        Debug.Log("[FINALIZE] Syncing inventory + hotbar + save");

        InvUI.Instance?.SyncToInventory();
        PlayerHotbarManager.Instance?.SyncHotbarToData();

        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();
    }

    public ItemData GetItem() => currentItem;
    public Ability GetAbility() => currentAbility;
    public int GetCount() => currentCount;
    public float GetDurability() => currentDurability;
}