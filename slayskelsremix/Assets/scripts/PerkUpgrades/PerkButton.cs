using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PerkButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buttonIcon;

    [Header("Highlight")]
    public Image background; // assign in inspector

    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.8f, 1f, 0.8f);   // light green
    public Color selectedColor = new Color(1f, 0.9f, 0.4f); // yellow

    [HideInInspector] public AbilityUpgradeSO perkAsset;

    private PerkManager _manager;
    private Ability _targetAbility;

    private bool isSelected;

    public void SetupButton(PerkManager manager, string abilityName, AbilityUpgradeSO asset)
    {
        _manager = manager;
        perkAsset = asset;

        if (manager.abilityDatabase != null)
        {
            _targetAbility = manager.abilityDatabase.GetAbilityByName(abilityName);

            if (_targetAbility != null && buttonIcon != null)
                buttonIcon.sprite = _targetAbility.icon;
        }

        SetNormal();
    }

    public void OnSelect()
    {
        Debug.Log($"[PerkButton] Selected → {_targetAbility?.abilityName}");

        if (_manager != null)
        {
            _manager.SelectPerk(perkAsset, _targetAbility);
            _manager.NotifySelection(this); // 🔥 important
        }
    }

    // 🟩 HOVER
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
            SetHover();

        if (_manager != null && perkAsset != null && _targetAbility != null)
        {
            _manager.DisplayPerkDetails(perkAsset, _targetAbility);
        }
    }

    // 🟥 EXIT
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            SetNormal();

        if (_manager != null)
        {
            _manager.RestoreSelectedPerk();
        }
    }

    // 🔥 VISUAL STATES
    public void SetSelected(bool value)
    {
        isSelected = value;

        if (isSelected)
            SetSelectedColor();
        else
            SetNormal();
    }

    private void SetNormal()
    {
        if (background != null)
            background.color = normalColor;
    }

    private void SetHover()
    {
        if (background != null)
            background.color = hoverColor;
    }

    private void SetSelectedColor()
    {
        if (background != null)
            background.color = selectedColor;
    }
}