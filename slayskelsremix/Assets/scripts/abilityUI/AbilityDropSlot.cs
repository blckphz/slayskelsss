using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class AbilityDropSlot : MonoBehaviour,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int slotIndex;

    [Header("UI References")]
    public Image slotIcon;
    public Image slotBackground;
    public GameObject ghostPrefab;

    [Tooltip("Drag the Spellbook/Ability Menu GameObject here")]
    public GameObject spellbookWindow;

    public Ability currentAbility;
    private bool isHovered;
    private GameObject ghost;
    private Canvas canvas;

    void Awake() => canvas = GetComponentInParent<Canvas>();

    void Update()
    {
        if (slotBackground == null) return;

        bool canDrag = spellbookWindow != null && spellbookWindow.activeInHierarchy;

        if (DragState.IsDraggingAbility && canDrag)
        {
            slotBackground.enabled = true;
            slotBackground.color = isHovered ? new Color(0f, 0f, 0f, 0.6f) : new Color(1f, 1f, 1f, 0.3f);
        }
        else
        {
            slotBackground.enabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (spellbookWindow == null || !spellbookWindow.activeInHierarchy) return;
        if (currentAbility == null) return;

        if (UIShaker.Instance != null) UIShaker.Instance.ShakeUI(0.1f, 5f);

        DragState.IsDraggingAbility = true;

        if (ghostPrefab != null)
        {
            ghost = Instantiate(ghostPrefab, canvas.transform);
            if (ghost.TryGetComponent<Image>(out var ghostImg))
                ghostImg.sprite = currentAbility.icon;

            CanvasGroup group = ghost.GetComponent<CanvasGroup>();
            if (group == null) group = ghost.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            ghost.transform.position = eventData.position;
            StartCoroutine(LerpGhostScale(ghost.transform, 0.85f, 0.15f));
        }

        if (slotIcon != null) slotIcon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null) ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragState.IsDraggingAbility = false;
        if (ghost != null) Destroy(ghost);
        if (slotIcon != null) slotIcon.color = Color.white;

        // FIXED: Clear text when drag ends
        PerkManager pm = Object.FindFirstObjectByType<PerkManager>();
        if (pm != null) pm.ClearPerkDetails();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (spellbookWindow == null || !spellbookWindow.activeInHierarchy) return;
        if (eventData.pointerDrag == null) return;

        AbilityUI draggedFromBook = eventData.pointerDrag.GetComponent<AbilityUI>();
        AbilityDropSlot draggedFromOtherSlot = eventData.pointerDrag.GetComponent<AbilityDropSlot>();

        bool success = false;

        if (draggedFromOtherSlot != null)
        {
            if (draggedFromOtherSlot == this) return;

            Ability incoming = draggedFromOtherSlot.currentAbility;
            Ability outgoing = this.currentAbility;

            AbilityLoadout.Instance.SetAbility(this.slotIndex, incoming);
            AbilityLoadout.Instance.SetAbility(draggedFromOtherSlot.slotIndex, outgoing);
            success = true;
        }
        else if (draggedFromBook != null)
        {
            Ability newAbility = draggedFromBook.ability;
            if (newAbility == null) return;

            for (int i = 0; i < AbilityLoadout.Instance.equippedAbilities.Length; i++)
            {
                if (i != slotIndex && AbilityLoadout.Instance.equippedAbilities[i] == newAbility)
                {
                    AbilityLoadout.Instance.SetAbility(i, null);
                }
            }
            AbilityLoadout.Instance.SetAbility(slotIndex, newAbility);
            success = true;
        }

        if (success && UIShaker.Instance != null)
            UIShaker.Instance.ShakeUI(0.15f, 12f);

        isHovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (spellbookWindow == null || !spellbookWindow.activeInHierarchy) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentAbility != null)
            {
                if (UIShaker.Instance != null)
                    UIShaker.Instance.ShakeUI(0.12f, 8f);

                AbilityLoadout.Instance.SetAbility(slotIndex, null);
            }
        }
    }

    public void UpdateSlotUI(Ability ability)
    {
        currentAbility = ability;
        if (slotIcon == null) return;

        if (ability == null)
        {
            slotIcon.enabled = false;
        }
        else
        {
            slotIcon.sprite = ability.icon;
            slotIcon.enabled = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        // FIXED: Only clear if we aren't dragging.
        if (!DragState.IsDraggingAbility)
        {
            PerkManager pm = Object.FindFirstObjectByType<PerkManager>();
            if (pm != null) pm.ClearPerkDetails();
        }
    }

    private IEnumerator LerpGhostScale(Transform target, float targetScale, float duration)
    {
        float time = 0;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = new Vector3(targetScale, targetScale, 1f);

        while (time < duration)
        {
            if (target == null) yield break;
            target.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        if (target != null) target.localScale = endScale;
    }
}