using UnityEngine;
using UnityEngine.EventSystems;

public class PerkButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public AbilityUpgradeSO perkAsset;
    public string targetAbilityName;

    private PerkUIDisplay _uiDisplay;
    private Ability _targetAbility;

    public void SetupButton(PerkManager manager, PerkUIDisplay display)
    {
        _uiDisplay = display;

        if (manager.abilityDatabase != null)
        {
            _targetAbility = manager.abilityDatabase.GetAbilityByName(targetAbilityName);

            if (_targetAbility == null)
                Debug.LogWarning($"<color=yellow>[PerkButton]</color> Ability '{targetAbilityName}' not found in DB for {gameObject.name}");
        }

        if (perkAsset == null)
            Debug.LogError($"<color=red>[PerkButton]</color> perkAsset is empty on {gameObject.name}!");
        else
            Debug.Log($"<color=green>[PerkButton]</color> {gameObject.name} linked to {perkAsset.UpgradeName}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_uiDisplay != null && _targetAbility != null && perkAsset != null)
        {
            _uiDisplay.UpdatePerkInfo(perkAsset, _targetAbility.icon);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_uiDisplay != null) _uiDisplay.ClearInfo();
    }

    public void OnClickUpgrade()
    {
        if (perkAsset != null && _targetAbility != null)
        {
            Debug.Log($"<color=white>[PerkButton]</color> Purchasing: {perkAsset.UpgradeName}");
            perkAsset.Apply(_targetAbility);

            // Close the menu (assuming buttons are inside a panel)
            // transform.parent.gameObject.SetActive(false); 
        }
    }
}