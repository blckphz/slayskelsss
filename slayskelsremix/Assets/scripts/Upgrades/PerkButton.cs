using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PerkButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Component")]
    public Image buttonIcon;
    public perkTooltip tooltip; // Assign the Tooltip object here in the inspector

    [HideInInspector] public AbilityUpgradeSO perkAsset;

    private string _targetAbilityName;
    private PerkUIDisplay _uiDisplay;
    private Ability _targetAbility;

    public void SetupButton(PerkManager manager, PerkUIDisplay display, string abilityName, AbilityUpgradeSO asset)
    {
        _uiDisplay = display;
        _targetAbilityName = abilityName;
        perkAsset = asset;

        if (manager.abilityDatabase != null)
        {
            _targetAbility = manager.abilityDatabase.GetAbilityByName(_targetAbilityName);
            if (_targetAbility != null && buttonIcon != null)
            {
                buttonIcon.sprite = _targetAbility.icon;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Update the main UI panel
        if (_uiDisplay != null && _targetAbility != null && perkAsset != null)
        {
            _uiDisplay.UpdatePerkInfo(perkAsset, _targetAbility.icon);
        }

        // Show the floating tooltip
        if (tooltip != null && perkAsset != null)
        {
            tooltip.ShowTooltip(perkAsset.UpgradeName, perkAsset.Description, perkAsset.Level, perkAsset.MaxLevel);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_uiDisplay != null) _uiDisplay.ClearInfo();

        // Hide floating tooltip
        if (tooltip != null) tooltip.HideTooltip();
    }

    public void OnClickUpgrade()
    {
        if (perkAsset == null || _targetAbility == null) return;

        if (perkAsset.Level < perkAsset.MaxLevel)
        {
            perkAsset.Apply(_targetAbility);
            perkAsset.Level++;
            perkAsset.SaveLevel();

            _uiDisplay.UpdatePerkInfo(perkAsset, _targetAbility.icon);

            // Refresh tooltip text after upgrade
            if (tooltip != null)
                tooltip.ShowTooltip(perkAsset.UpgradeName, perkAsset.Description, perkAsset.Level, perkAsset.MaxLevel);
        }
    }
}