using UnityEngine;
using UnityEngine.EventSystems;

public class abilityuislot : MonoBehaviour, IDropHandler
{
    public Ability assignedAbility;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        AbilityUI dragged = eventData.pointerDrag.GetComponent<AbilityUI>();

        if (dragged != null)
        {
            assignedAbility = dragged.ability;

            // Snap the UI element into the slot
            dragged.transform.SetParent(transform);
            dragged.transform.localPosition = Vector3.zero;

            // --- SHAKE UI ON SUCCESSFUL DROP ---
            if (UIShaker.Instance != null)
                UIShaker.Instance.ShakeUI(0.15f, 15f);

            Debug.Log("Assigned: " + assignedAbility.name);
        }
    }
}