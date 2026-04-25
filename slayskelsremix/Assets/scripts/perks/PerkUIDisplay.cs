using UnityEngine;
using UnityEngine.UI; // Required for the Image component
using TMPro;

public class PerkUIDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage; // Drag your UI Image component here

    [Header("Default Settings")]
    public Sprite defaultIcon; // A question mark or generic icon

    public void UpdatePerkInfo(IAbilityUpgrade perk, Sprite abilityIcon)
    {
        if (perk == null) return;

        titleText.text = perk.UpgradeName;
        descriptionText.text = perk.Description;
        
        // Set the icon from the ability
        if (iconImage != null)
        {
            iconImage.sprite = abilityIcon != null ? abilityIcon : defaultIcon;
            iconImage.enabled = true;
        }
    }

    public void ClearInfo()
    {
        titleText.text = "Select a Perk";
        descriptionText.text = "Hover over an icon to see details.";
        // Optional: hide the image or set to default
        if (iconImage != null) iconImage.sprite = defaultIcon;
    }
}