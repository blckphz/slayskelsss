using UnityEngine;
using UnityEngine.UI;

public class PerkUIDisplay : MonoBehaviour
{
    [Header("UI References")]
    public PerkManager PerkManager;
    public Image iconImage;
    public Button upgradeButton;

    [Header("Ability Target")]
    public Ability targetAbility; // The ability this perk modifies

    private IAbilityUpgrade currentPerk;

    public void UpdatePerkInfo(IAbilityUpgrade perk, Sprite abilityIcon)
    {
        if (perk == null) return;

        currentPerk = perk;

        PerkManager.titleText.text = $"{perk.UpgradeName} (LV {perk.Level}/{perk.MaxLevel})";
        PerkManager.descriptionText.text = perk.Description;

        if (iconImage != null)
            iconImage.sprite = abilityIcon;

        if (upgradeButton != null)
            upgradeButton.interactable = perk.Level < perk.MaxLevel;
    }

    // Called by Upgrade Button
    public void OnUpgradeClicked()
    {
        if (currentPerk == null) return;

        if (currentPerk.Level >= currentPerk.MaxLevel)
        {
            Debug.Log("Already at max level.");
            return;
        }

        // Increase level
        currentPerk.Level++;

        // Apply upgrade effect
        if (targetAbility != null)
        {
            currentPerk.Apply(targetAbility);
        }

        // Save progress (only if ScriptableObject)
        if (currentPerk is AbilityUpgradeSO so)
        {
            so.SaveLevel();

            // 🔥 Handle new ability unlock
            TryGrantNewAbility(so);
        }

        // Refresh UI
        UpdatePerkInfo(currentPerk, iconImage.sprite);

        Debug.Log($"Upgraded {currentPerk.UpgradeName} to Level {currentPerk.Level}");
    }

    private void TryGrantNewAbility(AbilityUpgradeSO so)
    {
        if (!so.givesNewAbility || so.newAbility == null)
            return;

        // Unlock ONLY at level 1
        if (so.Level != 1)
            return;

        if (AbilityUnlocks.Instance == null)
        {
            Debug.LogError("AbilityUnlocks missing in scene!");
            return;
        }

        AbilityUnlocks.Instance.UnlockAbility(so.newAbility);
    }

    public void ClearInfo()
    {
        currentPerk = null;

        PerkManager.titleText.text = "Select a Perk";
        PerkManager.descriptionText.text = "Hover over an icon to see details.";

        if (upgradeButton != null)
            upgradeButton.interactable = false;
    }
}