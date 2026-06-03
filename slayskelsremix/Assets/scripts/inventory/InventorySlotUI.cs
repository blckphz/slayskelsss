using System.Collections;
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
    public Image Background;
    public Image SlotItemPrevirew;

    [Header("Hover Settings")]
    public float hoverAlpha = 0.8f;
    public float normalAlpha = 0.35f;

    [Header("Rotation Settings")]
    public float maxRotation = 8f;

    [Header("Drag Preview Scale")]
    public float dragScale = 1.2f;
    public float scaleSpeed = 12f;

    [Header("Rotation Lerp")]
    public float rotationLerpSpeed = 12f;

    [Header("Shaker")]
    public float duration = 0.2f;
    public float strength = 6f;

    [Header("Audio")]
    public AudioClip dragStartClip;
    public AudioClip dragEndClip;
    public AudioClip dragHoverSlotClip;

    [Header("Drop Preview")]
    public Image slotDropPreviewIcon;

    [Header("Runtime State")]
    [SerializeField] private ItemData currentItem;
    [SerializeField] private Ability currentAbility;
    [SerializeField] private int currentCount;
    [SerializeField] private float currentDurability;

    public bool IsEmpty => currentItem == null && currentAbility == null;

    private Coroutine shakeRoutine;

    private bool isDragging = false;
    private bool isHovered = false;

    private RectTransform bgRect;
    private Quaternion targetRotation = Quaternion.identity;

    private Color originalBackgroundColor;

    private static InventorySlotUI lastHoveredDragSlot;

    private void Awake()
    {
        if (Background != null)
        {
            bgRect = Background.rectTransform;
            originalBackgroundColor = Background.color;

            var c = Background.color;
            c.a = normalAlpha;
            Background.color = c;
        }

        if (slotDropPreviewIcon != null)
        {
            slotDropPreviewIcon.enabled = false;
        }
    }

    private void Update()
    {
        if (dragPreviewIcon != null && dragPreviewIcon.enabled)
        {
            dragPreviewIcon.transform.localScale =
                Vector3.Lerp(
                    dragPreviewIcon.transform.localScale,
                    Vector3.one * dragScale,
                    Time.unscaledDeltaTime * scaleSpeed
                );
        }

        if (bgRect != null)
        {
            bgRect.rotation =
                Quaternion.Lerp(
                    bgRect.rotation,
                    targetRotation,
                    Time.unscaledDeltaTime * rotationLerpSpeed
                );
        }
    }

    public void ForceStopDrag()
    {
        isDragging = false;
        isHovered = false;

        lastHoveredDragSlot = null;

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.enabled = false;
            dragPreviewIcon.raycastTarget = false;
        }

        if (icon != null)
            icon.color = Color.white;

        HideDropPreview();

        targetRotation = Quaternion.identity;
        ApplyBackgroundState();

        invToolTip.Instance?.HideToolTip();
    }

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
        ApplyBackgroundState();
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

        if (countText != null)
            countText.enabled = false;

        if (durabilitySlider != null)
            durabilitySlider.gameObject.SetActive(false);

        ApplyBackgroundState();
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
                float percentage =
                    currentItem.maxDurability > 0
                        ? currentDurability / currentItem.maxDurability
                        : 0f;

                durabilityFill.color =
                    Color.Lerp(Color.red, Color.green, percentage);
            }
        }
        else if (durabilitySlider != null)
        {
            durabilitySlider.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            InventorySlotUI draggingSlot =
                eventData.pointerDrag.GetComponent<InventorySlotUI>()
                ?? eventData.pointerDrag.GetComponentInParent<InventorySlotUI>();

            if (draggingSlot != null && draggingSlot != this)
            {
                if (IsEmpty)
                    ShowDropPreview(draggingSlot);

                if (lastHoveredDragSlot != this)
                {
                    lastHoveredDragSlot = this;
                    AudioManager.Instance?.PlayUISound(dragHoverSlotClip);
                }
            }
        }

        if (isDragging) return;

        isHovered = true;
        ApplyBackgroundState();
        SetHoverVisual(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;

        isHovered = false;

        HideDropPreview();

        ApplyBackgroundState();
        SetHoverVisual(false);
    }

    private void ShowDropPreview(InventorySlotUI source)
    {
        if (slotDropPreviewIcon == null || source == null) return;

        var item = source.GetItem();
        var ability = source.GetAbility();

        slotDropPreviewIcon.enabled = true;
        slotDropPreviewIcon.raycastTarget = false;
        slotDropPreviewIcon.sprite = item != null ? item.icon : ability.icon;
        slotDropPreviewIcon.color = new Color(1f, 1f, 1f, 0.35f);

        // ✅ FIX: move preview to slot position
        RectTransform previewRect = slotDropPreviewIcon.rectTransform;
        RectTransform slotRect = GetComponent<RectTransform>();

        previewRect.position = slotRect.position;
    }

    private void HideDropPreview()
    {
        if (slotDropPreviewIcon != null)
            slotDropPreviewIcon.enabled = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        isDragging = true;
        isHovered = false;

        HideDropPreview();

        AudioManager.Instance?.PlayUISound(dragStartClip);

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.sprite = icon.sprite;
            dragPreviewIcon.enabled = true;
            dragPreviewIcon.raycastTarget = false;
            dragPreviewIcon.transform.position = eventData.position;
            dragPreviewIcon.transform.localScale = Vector3.one;
            ShakeIcon(dragPreviewIcon);
        }

        if (icon != null)
            icon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreviewIcon != null && dragPreviewIcon.enabled)
            dragPreviewIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        lastHoveredDragSlot = null;

        AudioManager.Instance?.PlayUISound(dragEndClip);

        if (dragPreviewIcon != null)
            dragPreviewIcon.enabled = false;

        if (icon != null)
            icon.color = Color.white;

        HideDropPreview();

        targetRotation = Quaternion.identity;
        ApplyBackgroundState();
    }

    public void OnDrop(PointerEventData eventData)
    {
        HideDropPreview();

        if (eventData.pointerDrag == null)
            return;

        InventorySlotUI source =
            eventData.pointerDrag.GetComponent<InventorySlotUI>()
            ?? eventData.pointerDrag.GetComponentInParent<InventorySlotUI>();

        if (source != null && source != this)
        {
            var tempItem = currentItem;
            var tempAbility = currentAbility;
            var tempCount = currentCount;
            var tempDurability = currentDurability;

            SetSlot(source.GetItem(), source.GetAbility(), source.GetCount(), source.GetDurability());
            source.SetSlot(tempItem, tempAbility, tempCount, tempDurability);

            FinalizeStackTransaction(source);
        }
    }

    private void SetHoverVisual(bool state)
    {
        if (Background != null)
        {
            var c = Background.color;
            c.a = state ? hoverAlpha : normalAlpha;
            Background.color = c;
        }

        targetRotation = state
            ? Quaternion.Euler(0f, 0f, Random.Range(-maxRotation, maxRotation))
            : Quaternion.identity;

        if (state && currentItem != null)
            invToolTip.Instance?.ShowToolTip(currentItem, currentDurability);
        else
            invToolTip.Instance?.HideToolTip();
    }

    private void ApplyBackgroundState()
    {
        if (Background == null) return;

        var c = Background.color;
        c.a = isDragging ? 1f : (isHovered ? hoverAlpha : normalAlpha);
        Background.color = c;
    }

    private void ShakeIcon(Image target)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeCoroutine(target.rectTransform));
    }

    private IEnumerator ShakeCoroutine(RectTransform rect)
    {
        Vector3 original = rect.anchoredPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            rect.anchoredPosition =
                original + new Vector3(
                    Random.Range(-strength, strength),
                    Random.Range(-strength, strength),
                    0
                );

            yield return null;
        }

        rect.anchoredPosition = original;
    }

    private void FinalizeStackTransaction(InventorySlotUI source)
    {
        InvUI.Instance?.SyncToInventory();
        PlayerHotbarManager.Instance?.SyncHotbarToData();
        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();
        invToolTip.Instance?.HideToolTip();
    }

    public ItemData GetItem() => currentItem;
    public Ability GetAbility() => currentAbility;
    public int GetCount() => currentCount;
    public float GetDurability() => currentDurability;
}