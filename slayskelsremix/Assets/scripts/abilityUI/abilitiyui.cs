using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class AbilityUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Ability ability;
    public Image icon;
    public AbilityUpgradeSO linkedPerk;
    public Image ghostPrefab;

    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public static bool IsHoveringAbility;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (icon != null && ability != null)
            icon.sprite = ability.icon;
    }

    public void OnPointerEnter(PointerEventData eventData) => IsHoveringAbility = true;

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHoveringAbility = false;

        // Attempt to clear, but PerkManager will block this if we are dragging
        PerkManager pm = Object.FindFirstObjectByType<PerkManager>();
        if (pm != null) pm.ClearPerkDetails();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PerkManager pm = Object.FindFirstObjectByType<PerkManager>();
        if (pm != null && ability != null && linkedPerk != null)
        {
            pm.SelectPerk(linkedPerk, ability);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (linkedPerk != null && linkedPerk.Level <= 0) return;

        // Set global dragging state
        DragState.IsDraggingAbility = true;

        if (UIShaker.Instance != null)
            UIShaker.Instance.ShakeUI(0.1f, 6f);

        if (ghostPrefab != null)
        {
            ghost = Instantiate(ghostPrefab.gameObject, canvas.transform);
            if (icon != null) ghost.GetComponent<Image>().sprite = icon.sprite;

            ghost.transform.position = eventData.position;

            // Start the size lerp effect
            StartCoroutine(LerpGhostScale(ghost.transform, 0.85f, 0.15f));
        }

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null) ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Reset state BEFORE calling clear
        DragState.IsDraggingAbility = false;

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        if (ghost != null) Destroy(ghost);

        // Now that IsDraggingAbility is false, this call will finally succeed
        PerkManager pm = Object.FindFirstObjectByType<PerkManager>();
        if (pm != null) pm.ClearPerkDetails();
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