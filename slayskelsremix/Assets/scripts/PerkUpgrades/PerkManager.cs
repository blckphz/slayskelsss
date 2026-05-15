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
        if (abilityDatabase == null) return;
        InitializeAllSlots();
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    private void OnEnable() => toggleAction.action.Enable();
    private void OnDisable() => toggleAction.action.Disable();

    private void Update()
    {
        if (toggleAction.action.WasPressedThisFrame()) ToggleUpgradeUI();
    }

    public void ToggleUpgradeUI()
    {
        if (upgradePanel == null) return;
        bool isActive = upgradePanel.activeSelf;
        upgradePanel.SetActive(!isActive);

        if (isActive)
        {
            // Force clear when closing UI
            titleText.text = "";
            descriptionText.text = "";
            _selectedPerk = null;
            _selectedAbility = null;
        }
    }

    private void InitializeAllSlots()
    {
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
        DisplayPerkDetails(perkAsset, ability);
    }

    public void ConfirmUpgrade()
    {
        if (_selectedPerk == null || _selectedAbility == null) return;

        if (_selectedPerk.Level < _selectedPerk.MaxLevel)
        {
            _selectedPerk.Apply(_selectedAbility);
            _selectedPerk.Level++;
            _selectedPerk.SaveLevel();

            if (UIShaker.Instance != null)
                UIShaker.Instance.ShakeUI(0.2f, 20f);

            DisplayPerkDetails(_selectedPerk, _selectedAbility);
        }
    }

    public void DisplayPerkDetails(AbilityUpgradeSO perkAsset, Ability ability)
    {
        if (titleText == null || descriptionText == null) return;

        titleText.text = ability.abilityName;
        string fullStats = $"<b>--- Base Stats ---</b>\nUse Speed: {ability.fireRate}s\n";

        if (ability is offensiveability offensive)
            fullStats += $"Base DMG: <color=#FF5555>{offensive.damage}</color>\n";

        if (ability is IStatProvider provider)
            fullStats += provider.GetStatsFormat() + "\n";

        var perkInfo = perkAsset.GetDisplayStrings();
        fullStats += $"\n<b>Current Level ({perkAsset.Level}/{perkAsset.MaxLevel}):</b>\n{perkInfo.displayDesc}";
        descriptionText.text = fullStats;
    }

    public void ClearPerkDetails()
    {
        // CRITICAL FIX: If the player is currently dragging, refuse to clear.
        if (DragState.IsDraggingAbility) return;

        if (titleText != null) titleText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }

    public void RestoreSelectedPerk()
    {
        if (_selectedPerk != null && _selectedAbility != null)
            DisplayPerkDetails(_selectedPerk, _selectedAbility);
        else
            ClearPerkDetails();
    }

    public void NotifySelection(PerkButton selectedButton)
    {
        foreach (PerkSlot slot in perkSlots)
        {
            if (slot.button != null)
                slot.button.SetSelected(slot.button == selectedButton);
        }
    }
}