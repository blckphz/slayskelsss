using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public Ability ability;
    public Image icon;

    [Header("Perk Integration")]
    public PerkUIDisplay perkDisplay;
    public AbilityUpgradeSO linkedPerk;

    [Header("Ghost Settings")]
    public Image ghostPrefab;

    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private bool canDrag = true;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (perkDisplay == null)
            perkDisplay = FindFirstObjectByType<PerkUIDisplay>();

        if (icon != null && ability != null)
            icon.sprite = ability.icon;
    }

    // ---------------- HOVER ----------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (perkDisplay != null && linkedPerk != null)
            perkDisplay.UpdatePerkInfo(linkedPerk, ability.icon);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (perkDisplay != null)
            perkDisplay.ClearInfo();
    }

    // ---------------- DRAG ----------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        DragState.IsDraggingAbility = true; // 🔥 ADD THIS

        if (linkedPerk != null && linkedPerk.Level <= 0)
        {
            Debug.LogWarning("[DRAG BLOCKED] Level 0 ability");
            return;
        }

        if (ghostPrefab != null)
        {
            ghost = Instantiate(ghostPrefab.gameObject, canvas.transform);

            Image img = ghost.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = icon.sprite;
                img.raycastTarget = false;
                img.color = new Color(1f, 1f, 1f, 0.6f);
            }

            ghost.transform.position = eventData.position;
        }

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (ghost != null)
            Destroy(ghost);

        // IMPORTANT:
        // NO repositioning of original icon
        // NO transform.localPosition changes
    }
}