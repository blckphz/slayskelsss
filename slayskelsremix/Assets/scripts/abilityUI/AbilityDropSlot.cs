using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityDropSlot : MonoBehaviour,
    IDropHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public int slotIndex;

    [Header("UI")]
    public Image slotIcon;
    public Image slotBackground;

    private bool isHovered;

    void Update()
    {
        if (slotBackground == null) return;

        if (DragState.IsDraggingAbility)
        {
            slotBackground.enabled = true;

            // 🖤 hovered slot darker
            if (isHovered)
                slotBackground.color = new Color(0f, 0f, 0f, 0.6f);
            else
                slotBackground.color = new Color(1f, 1f, 1f, 0.3f);
        }
        else
        {
            slotBackground.enabled = false;
        }
    }

    // 🟩 hover enter
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    // 🟥 hover exit
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    // 💥 drop
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        AbilityUI dragged = eventData.pointerDrag.GetComponent<AbilityUI>();
        if (dragged == null || dragged.ability == null) return;

        if (AbilityLoadout.Instance == null) return;

        // 🔒 unlock check
        if (AbilityUnlocks.Instance != null &&
            !AbilityUnlocks.Instance.IsUnlocked(dragged.ability))
        {
            Debug.LogWarning("[Drop] Locked ability");
            return;
        }

        // 🚫 duplicate check
        for (int i = 0; i < AbilityLoadout.Instance.equippedAbilities.Length; i++)
        {
            if (i == slotIndex) continue;

            if (AbilityLoadout.Instance.equippedAbilities[i] == dragged.ability)
            {
                Debug.LogWarning("[Drop] Already equipped");
                return;
            }
        }

        AbilityLoadout.Instance.SetAbility(slotIndex, dragged.ability);
        UpdateSlotUI(dragged.ability);

        isHovered = false;
    }

    // 🖱️ RIGHT CLICK REMOVE
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        RemoveAbility();
    }

    private void RemoveAbility()
    {
        if (AbilityLoadout.Instance == null) return;

        AbilityLoadout.Instance.SetAbility(slotIndex, null);

        if (slotIcon != null)
        {
            slotIcon.sprite = null;
            slotIcon.enabled = false;
        }

        Debug.Log($"[Slot] Removed ability from slot {slotIndex}");
    }

    private void UpdateSlotUI(Ability ability)
    {
        if (ability == null)
        {
            if (slotIcon != null)
                slotIcon.enabled = false;
            return;
        }

        if (slotIcon != null)
        {
            slotIcon.sprite = ability.icon;
            slotIcon.enabled = true;
        }
    }
}