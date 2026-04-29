using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Added PointerEnter and PointerExit for the Perk UI
public class AbilityUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Ability ability;
    public Image icon;

    [Header("Perk Integration")]
    // If your perks are stored inside the Ability script, or matched by ID
    public PerkUIDisplay perkDisplay;

    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    public bool dropped;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (perkDisplay == null) perkDisplay = FindFirstObjectByType<PerkUIDisplay>();

        if (icon != null && ability != null)
            icon.sprite = ability.icon;
    }

    // --- PERK UI HOVER LOGIC ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (perkDisplay != null && ability != null)
        {
            // We cast the ability to IAbilityUpgrade if it implements it, 
            // or pass the perk reference associated with this ability.
            if (ability is IAbilityUpgrade perk)
            {
                perkDisplay.UpdatePerkInfo(perk, ability.icon);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (perkDisplay != null)
        {
            perkDisplay.ClearInfo();
        }
    }

    // --- DRAG LOGIC (Keep your existing logic) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        dropped = false;
        originalParent = transform.parent;
        transform.SetParent(canvas.transform);
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        if (!dropped)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }
}