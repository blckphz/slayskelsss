using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class PerkManager : MonoBehaviour
{
    [Header("UI Panel Toggle")]
    public GameObject upgradePanel;
    public InputActionProperty toggleAction;

    [Header("Main Info Panel (Detail Panel)")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Settings")]
    public AbilityDatabase abilityDatabase;

    [Header("Master Perk List")]
    public List<PerkSlot> perkSlots = new List<PerkSlot>();

    private AbilityUpgradeSO _selectedPerk;
    private Ability _selectedAbility;

    private void Awake()
    {
        Debug.Log("[PerkManager] Awake");

        if (abilityDatabase == null)
        {
            Debug.LogError("[PerkManager] AbilityDatabase missing!");
            return;
        }

        InitializeAllSlots();

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    private void OnEnable()
    {
        Debug.Log("[PerkManager] Input Enabled");
        toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        Debug.Log("[PerkManager] Input Disabled");
        toggleAction.action.Disable();
    }

    private void Update()
    {
        if (toggleAction.action.WasPressedThisFrame())
        {
            Debug.Log("[PerkManager] Toggle pressed");
            ToggleUpgradeUI();
        }
    }

    public void ToggleUpgradeUI()
    {
        if (upgradePanel == null) return;

        bool isActive = upgradePanel.activeSelf;

        Debug.Log($"[PerkManager] Toggle UI → {(isActive ? "Closing" : "Opening")}");

        upgradePanel.SetActive(!isActive);

        if (isActive)
        {
            Debug.Log("[PerkManager] Clearing UI + Selection");

            ClearPerkDetails();

            // 🔥 IMPORTANT FIX
            _selectedPerk = null;
            _selectedAbility = null;
        }
    }

    private void InitializeAllSlots()
    {
        Debug.Log("[PerkManager] Initializing slots");

        foreach (PerkSlot slot in perkSlots)
        {
            if (slot.perk != null && slot.button != null)
            {
                slot.perk.LoadLevel();
                slot.button.SetupButton(this, slot.targetAbility, slot.perk);
            }
        }
    }

    public void SelectPerk(AbilityUpgradeSO perkAsset, Ability ability)
    {
        if (perkAsset == null || ability == null) return;

        _selectedPerk = perkAsset;
        _selectedAbility = ability;

        Debug.Log($"[PerkManager] Selected {ability.abilityName}");

        DisplayPerkDetails(perkAsset, ability);
    }

    public void ConfirmUpgrade()
    {
        if (_selectedPerk == null || _selectedAbility == null)
        {
            Debug.LogWarning("[PerkManager] No perk selected for upgrade");
            return;
        }

        if (_selectedPerk.Level < _selectedPerk.MaxLevel)
        {
            _selectedPerk.Apply(_selectedAbility);
            _selectedPerk.Level++;
            _selectedPerk.SaveLevel();

            Debug.Log($"[PerkManager] Upgraded {_selectedAbility.abilityName} → Level {_selectedPerk.Level}");

            DisplayPerkDetails(_selectedPerk, _selectedAbility);
        }
        else
        {
            Debug.Log("[PerkManager] Already at max level");
        }
    }

    public void DisplayPerkDetails(AbilityUpgradeSO perkAsset, Ability ability)
    {
        if (titleText == null || descriptionText == null) return;

        Debug.Log($"[PerkManager] Display → {ability.abilityName} (Level {perkAsset.Level})");

        titleText.text = ability.abilityName;

        string fullStats = $"<b>--- Base Stats ---</b>\n";
        fullStats += $"Use Speed: {ability.fireRate}s\n";

        if (ability is offensiveability offensive)
        {
            fullStats += $"Base DMG: <color=#FF5555>{offensive.damage}</color>\n";
        }

        if (ability is IStatProvider provider)
        {
            fullStats += provider.GetStatsFormat() + "\n";
        }

        var perkInfo = perkAsset.GetDisplayStrings();

        fullStats += $"\n<b>Current Level ({perkAsset.Level}/{perkAsset.MaxLevel}):</b>\n{perkInfo.displayDesc}";

        descriptionText.text = fullStats;
    }

    // 🧹 CLEAR PANEL
    public void ClearPerkDetails()
    {
        Debug.Log("[PerkManager] Clearing UI text");

        if (titleText != null)
            titleText.text = "";

        if (descriptionText != null)
            descriptionText.text = "";
    }

    // 🔄 RESTORE SELECTED OR CLEAR
    public void RestoreSelectedPerk()
    {
        if (_selectedPerk != null && _selectedAbility != null)
        {
            Debug.Log("[PerkManager] Restoring selected perk");

            DisplayPerkDetails(_selectedPerk, _selectedAbility);
        }
        else
        {
            Debug.Log("[PerkManager] No selection → clearing");

            ClearPerkDetails();
        }
    }
    public void NotifySelection(PerkButton selectedButton)
    {
        Debug.Log("[PerkManager] Updating button highlights");

        foreach (PerkSlot slot in perkSlots)
        {
            if (slot.button == null) continue;

            bool isThis = slot.button == selectedButton;
            slot.button.SetSelected(isThis);
        }
    }

}