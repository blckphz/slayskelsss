using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerkUIDisplay : MonoBehaviour
{
    [Header("UI References")]
    public PerkManager PerkManager;
    public Image iconImage;
    public Button upgradeButton; // Add an upgrade button in the UI

    private IAbilityUpgrade currentPerk;

    public void UpdatePerkInfo(IAbilityUpgrade perk, Sprite abilityIcon)
    {
        if (perk == null) return;
        currentPerk = perk;

        PerkManager.titleText.text = $"{perk.UpgradeName} (LV {perk.Level}/{perk.MaxLevel})";
        PerkManager.descriptionText.text = perk.Description;

        if (iconImage != null) iconImage.sprite = abilityIcon;

        // Enable button if not max level
        if (upgradeButton != null)
            upgradeButton.interactable = perk.Level < perk.MaxLevel;
    }

    // Hook this up to the Upgrade Button's OnClick in the Inspector
    public void OnUpgradeClicked()
    {
        if (currentPerk != null && currentPerk.Level < currentPerk.MaxLevel)
        {
            // Assuming your perk interface has an ApplyUpgrade or similar method
            // currentPerk.Level++; 

            // Refresh display
            UpdatePerkInfo(currentPerk, iconImage.sprite);
            Debug.Log($"Upgraded {currentPerk.UpgradeName} to Level {currentPerk.Level}");
        }
    }

    public void ClearInfo()
    {
        currentPerk = null;
        PerkManager.titleText.text = "Select a Perk";
        PerkManager.descriptionText.text = "Hover over an icon to see details.";
        if (upgradeButton != null) upgradeButton.interactable = false;
    }
}