using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Ability ability;
    public Image icon;
    public AbilityUpgradeSO linkedPerk;
    public Image ghostPrefab;

    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (icon != null && ability != null) icon.sprite = ability.icon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[AbilityUI] Clicked {ability?.name}");
        PerkManager pm = Object.FindFirstObjectByType<PerkManager>();
        if (pm != null && ability != null && linkedPerk != null)
        {
            pm.SelectPerk(linkedPerk, ability);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (linkedPerk != null && linkedPerk.Level <= 0) return;

        DragState.IsDraggingAbility = true;
        if (ghostPrefab != null)
        {
            ghost = Instantiate(ghostPrefab.gameObject, canvas.transform);
            ghost.GetComponent<Image>().sprite = icon.sprite;
            ghost.transform.position = eventData.position;
        }
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null) ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragState.IsDraggingAbility = false;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        if (ghost != null) Destroy(ghost);
    }
}