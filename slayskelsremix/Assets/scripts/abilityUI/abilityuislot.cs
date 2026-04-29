using UnityEngine;
using UnityEngine.EventSystems;

public class abilityuislot : MonoBehaviour, IDropHandler
{
    public Ability assignedAbility;

    public void OnDrop(PointerEventData eventData)
    {
        AbilityUI dragged = eventData.pointerDrag.GetComponent<AbilityUI>();

        if (dragged != null)
        {
            assignedAbility = dragged.ability;

            Debug.Log("Assigned: " + assignedAbility.name);

            // Optional: update icon
            // GetComponent<Image>().sprite = dragged.icon;

            // Snap the UI element into the slot
            dragged.transform.SetParent(transform);
            dragged.transform.localPosition = Vector3.zero;
        }
    }
}