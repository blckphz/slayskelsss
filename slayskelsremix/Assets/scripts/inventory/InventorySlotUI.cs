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

    [Header("Hover Settings")]
    public float hoverAlpha = 1f;
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

    // ================= UNITY =================

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
    }

    private void Update()
    {
        if (dragPreviewIcon != null && dragPreviewIcon.enabled)
        {
            dragPreviewIcon.transform.localScale = Vector3.Lerp(
                dragPreviewIcon.transform.localScale,
                Vector3.one * dragScale,
                Time.unscaledDeltaTime * scaleSpeed
            );
        }

        if (bgRect != null)
        {
            bgRect.rotation = Quaternion.Lerp(
                bgRect.rotation,
                targetRotation,
                Time.unscaledDeltaTime * rotationLerpSpeed
            );
        }
    }

    // ================= PUBLIC FORCE STOP =================

    public void ForceStopDrag()
    {
        isDragging = false;
        isHovered = false;

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.enabled = false;
            dragPreviewIcon.raycastTarget = false;
        }

        if (icon != null)
            icon.color = Color.white;

        targetRotation = Quaternion.identity;
        ApplyBackgroundState();

        invToolTip.Instance?.HideToolTip();
    }

    // ================= SET =================

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

        if (countText != null) countText.enabled = false;
        if (durabilitySlider != null) durabilitySlider.gameObject.SetActive(false);

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
                        ? (currentDurability / currentItem.maxDurability)
                        : 0f;

                durabilityFill.color = Color.Lerp(Color.red, Color.green, percentage);
            }
        }
        else
        {
            if (durabilitySlider != null)
                durabilitySlider.gameObject.SetActive(false);
        }
    }

    // ================= POINTER =================

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;

        isHovered = true;
        ApplyBackgroundState();
        SetHoverVisual(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;

        isHovered = false;
        ApplyBackgroundState();
        SetHoverVisual(false);
    }

    private void SetHoverVisual(bool state)
    {
        if (Background != null)
        {
            var c = Background.color;
            c.a = state ? hoverAlpha : normalAlpha;
            Background.color = c;
        }

        if (state)
        {
            float angle = Random.Range(-maxRotation, maxRotation);
            targetRotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            targetRotation = Quaternion.identity;
        }

        if (state && invToolTip.Instance != null && currentItem != null)
            invToolTip.Instance.ShowToolTip(currentItem, currentDurability);

        if (!state)
            invToolTip.Instance?.HideToolTip();
    }

    // ================= DRAG =================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        isDragging = true;
        isHovered = false;

        invToolTip.Instance?.HideToolTip();

        if (Background != null)
        {
            var c = Background.color;
            c.a = 1f;
            Background.color = c;
        }

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

        if (dragPreviewIcon != null)
        {
            dragPreviewIcon.enabled = false;
            ShakeIcon(dragPreviewIcon);
        }

        if (icon != null)
            icon.color = Color.white;

        targetRotation = Quaternion.identity;
        ApplyBackgroundState();
    }

    // ================= DROP =================

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        InventorySlotUI sourcePlayerSlot =
            eventData.pointerDrag.GetComponent<InventorySlotUI>()
            ?? eventData.pointerDrag.GetComponentInParent<InventorySlotUI>();

        if (sourcePlayerSlot != null && sourcePlayerSlot != this)
        {
            ItemData targetItem = currentItem;
            Ability targetAbility = currentAbility;
            int targetCount = currentCount;
            float targetDurability = currentDurability;

            ItemData sourceItem = sourcePlayerSlot.GetItem();
            Ability sourceAbility = sourcePlayerSlot.GetAbility();
            int sourceCount = sourcePlayerSlot.GetCount();
            float sourceDurability = sourcePlayerSlot.GetDurability();

            SetSlot(sourceItem, sourceAbility, sourceCount, sourceDurability);
            sourcePlayerSlot.SetSlot(targetItem, targetAbility, targetCount, targetDurability);

            ApplyBackgroundState();
            sourcePlayerSlot.ApplyBackgroundState();

            FinalizeStackTransaction(sourcePlayerSlot);
        }

        ApplyBackgroundState();
    }

    // ================= STATE =================

    private void ApplyBackgroundState()
    {
        if (Background == null) return;

        var c = Background.color;

        if (isDragging)
            c.a = 1f;
        else
            c.a = isHovered ? hoverAlpha : normalAlpha;

        Background.color = c;
    }

    // ================= SHAKE =================

    private void ShakeIcon(Image target)
    {
        if (target == null) return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeCoroutine(target.rectTransform));
    }

    private IEnumerator ShakeCoroutine(RectTransform rect)
    {
        Vector3 originalPos = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float x = Random.Range(-strength, strength);
            float y = Random.Range(-strength, strength);

            rect.anchoredPosition = originalPos + new Vector3(x, y, 0);

            yield return null;
        }

        rect.anchoredPosition = originalPos;
        shakeRoutine = null;
    }

    // ================= FINALIZE =================

    private void FinalizeStackTransaction(InventorySlotUI sourcePlayerSlot)
    {
        InvUI.Instance?.SyncToInventory();
        PlayerHotbarManager.Instance?.SyncHotbarToData();

        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();

        invToolTip.Instance?.HideToolTip();
    }

    // ================= GETTERS =================

    public ItemData GetItem() => currentItem;
    public Ability GetAbility() => currentAbility;
    public int GetCount() => currentCount;
    public float GetDurability() => currentDurability;
}