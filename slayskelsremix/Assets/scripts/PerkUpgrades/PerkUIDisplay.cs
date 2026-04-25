using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkUIDisplay : MonoBehaviour
{
    [Header("UI References")]
    public PerkManager PerkManager;
    public Image iconImage;

    public void UpdatePerkInfo(IAbilityUpgrade perk, Sprite abilityIcon)
    {
        if (perk == null) return;

        PerkManager.titleText.text = $"{perk.UpgradeName} (LV {perk.Level}/{perk.MaxLevel})";
        PerkManager.descriptionText.text = perk.Description;

        if (iconImage != null)
        {
            iconImage.sprite = abilityIcon;
        }
    }

    public void ClearInfo()
    {
        PerkManager.titleText.text = "Select a Perk";
        PerkManager.descriptionText.text = "Hover over an icon to see details.";
    }
}