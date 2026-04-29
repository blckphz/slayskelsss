using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityDropSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[Drop] Object dropped onto Slot {slotIndex}");

        if (eventData.pointerDrag == null) return;

        AbilityUI draggedAbility = eventData.pointerDrag.GetComponent<AbilityUI>();
        if (draggedAbility == null) return;

        // --- THE SAFETY CHECK ---
        if (AbilityLoadout.Instance == null)
        {
            Debug.LogError("[Drop] CRITICAL: AbilityLoadout.Instance is NULL! Is it in your scene?");
            return;
        }

        // If we get here, everything is safe
        AbilityLoadout.Instance.SetAbility(slotIndex, draggedAbility.ability);

        // This is the line that was being skipped because of the crash:
        draggedAbility.dropped = true;

        draggedAbility.transform.SetParent(transform);
        draggedAbility.transform.localPosition = Vector3.zero;

        Debug.Log($"[Drop] SUCCESS! Equipped {draggedAbility.ability.name} to slot {slotIndex}");
    }
}