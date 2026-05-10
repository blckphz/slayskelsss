using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Ability ability;
    public Image icon;
    public AbilityUpgradeSO linkedPerk;
    public Image ghostPrefab;

    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    // 🔥 GLOBAL FLAG TO PREVENT UI CONFLICTS
    public static bool IsHoveringAbility;

    void Awake()
    {
        Debug.Log($"[AbilityUI] AWAKE -> {gameObject.name}");

        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (icon != null && ability != null)
            icon.sprite = ability.icon;

        if (ability == null)
            Debug.LogError($"[AbilityUI] ❌ Missing ability on {gameObject.name}");
    }

    void Start()
    {
        Debug.Log($"[AbilityUI] START -> {ability?.name}");
    }

    // =========================
    // HOVER ENTER
    // =========================
    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHoveringAbility = true;

        Debug.Log($"[AbilityUI] HOVER ENTER -> {ability?.name}");

        if (ability == null)
        {
            Debug.LogWarning("[AbilityUI] Ability is NULL");
            return;
        }

        Debug.Log($"[AbilityUI] DESCRIPTION: {ability.description}");
    }

    // =========================
    // HOVER EXIT
    // =========================
    public void OnPointerExit(PointerEventData eventData)
    {
        IsHoveringAbility = false;

        Debug.Log($"[AbilityUI] HOVER EXIT -> {ability?.name}");
    }

    // =========================
    // CLICK
    // =========================
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[AbilityUI] CLICK -> {ability?.name}");

        PerkManager pm = Object.FindFirstObjectByType<PerkManager>();

        if (pm == null)
        {
            Debug.LogWarning("[AbilityUI] PerkManager NOT FOUND");
            return;
        }

        if (ability == null || linkedPerk == null)
        {
            Debug.LogWarning("[AbilityUI] Missing ability or perk");
            return;
        }

        pm.SelectPerk(linkedPerk, ability);
    }

    // =========================
    // DRAG START
    // =========================
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[AbilityUI] DRAG START -> {ability?.name}");

        if (linkedPerk != null && linkedPerk.Level <= 0)
        {
            Debug.LogWarning("[AbilityUI] Cannot drag (perk level too low)");
            return;
        }

        DragState.IsDraggingAbility = true;

        if (ghostPrefab != null)
        {
            ghost = Instantiate(ghostPrefab.gameObject, canvas.transform);

            if (icon != null)
                ghost.GetComponent<Image>().sprite = icon.sprite;

            ghost.transform.position = eventData.position;

            Debug.Log("[AbilityUI] Ghost created");
        }

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    // =========================
    // DRAGGING
    // =========================
    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    // =========================
    // DRAG END
    // =========================
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[AbilityUI] DRAG END -> {ability?.name}");

        DragState.IsDraggingAbility = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (ghost != null)
        {
            Destroy(ghost);
            Debug.Log("[AbilityUI] Ghost destroyed");
        }
    }
}