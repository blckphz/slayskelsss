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

        if (abilityDatabase == null)
        {
            Debug.LogError("[PerkManager] AbilityDatabase is NULL!");
            return;
        }

        if (upgradePanel == null)
        {
            Debug.LogError("[PerkManager] Upgrade Panel is NULL!");
        }

        InitializeAllSlots();

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    private void OnEnable()
    {

        if (toggleAction.action == null)
        {
            Debug.LogError("[PerkManager] Toggle action is NULL!");
            return;
        }

        toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        if (toggleAction.action != null)
            toggleAction.action.Disable();
    }

    private void Update()
    {
        if (toggleAction.action == null)
            return;

        if (toggleAction.action.WasPressedThisFrame())
        {
            Debug.Log("[PerkManager] Toggle key pressed.");
            ToggleUpgradeUI();
        }
    }

    public void ToggleUpgradeUI()
    {
        Debug.Log("[PerkManager] ToggleUpgradeUI called.");

        if (upgradePanel == null)
        {
            Debug.LogError("[PerkManager] Cannot toggle: upgradePanel is NULL.");
            return;
        }

        bool isActive = upgradePanel.activeSelf;

        Debug.Log($"[PerkManager] Current panel state: {isActive}");

        upgradePanel.SetActive(!isActive);

        Debug.Log($"[PerkManager] New panel state: {upgradePanel.activeSelf}");

        if (isActive)
        {
            Debug.Log("[PerkManager] Closing panel, clearing selection.");

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
            if (slot == null)
            {
                Debug.LogWarning("[PerkManager] Null perk slot found.");
                continue;
            }

            if (slot.perk == null)
            {
                Debug.LogWarning("[PerkManager] Slot has null perk.");
                continue;
            }

            if (slot.button == null)
            {
                Debug.LogWarning("[PerkManager] Slot has null button.");
                continue;
            }


            slot.perk.LoadLevel();
            slot.button.SetupButton(this, slot.targetAbility, slot.perk);
        }
    }

    public void SelectPerk(AbilityUpgradeSO perkAsset, Ability ability)
    {
        Debug.Log($"[PerkManager] SelectPerk: {perkAsset?.name}");

        if (perkAsset == null || ability == null)
        {
            Debug.LogWarning("[PerkManager] SelectPerk failed: null values.");
            return;
        }

        _selectedPerk = perkAsset;
        _selectedAbility = ability;

        DisplayPerkDetails(perkAsset, ability);
    }

    public void ConfirmUpgrade()
    {
        Debug.Log("[PerkManager] ConfirmUpgrade");

        if (_selectedPerk == null || _selectedAbility == null)
        {
            Debug.LogWarning("[PerkManager] No perk selected.");
            return;
        }

        if (_selectedPerk.Level < _selectedPerk.MaxLevel)
        {
            Debug.Log($"[PerkManager] Applying upgrade: {_selectedPerk.name}");

            _selectedPerk.Apply(_selectedAbility);
            _selectedPerk.Level++;
            _selectedPerk.SaveLevel();

            if (UIShaker.Instance != null)
                UIShaker.Instance.ShakeUI(0.2f, 20f);

            DisplayPerkDetails(_selectedPerk, _selectedAbility);
        }
        else
        {
            Debug.Log("[PerkManager] Perk already maxed.");
        }
    }

    public void DisplayPerkDetails(AbilityUpgradeSO perkAsset, Ability ability)
    {
        Debug.Log("[PerkManager] DisplayPerkDetails");

        if (titleText == null || descriptionText == null)
        {
            Debug.LogError("[PerkManager] UI text references missing.");
            return;
        }

        titleText.text = ability.abilityName;

        string fullStats =
            $"<b>--- Base Stats ---</b>\nUse Speed: {ability.fireRate}s\n";

        if (ability is offensiveability offensive)
            fullStats += $"Base DMG: <color=#FF5555>{offensive.damage}</color>\n";

        if (ability is IStatProvider provider)
            fullStats += provider.GetStatsFormat() + "\n";

        var perkInfo = perkAsset.GetDisplayStrings();

        fullStats +=
            $"\n<b>Current Level ({perkAsset.Level}/{perkAsset.MaxLevel}):</b>\n{perkInfo.displayDesc}";

        descriptionText.text = fullStats;
    }

    public void ClearPerkDetails()
    {
        if (DragState.IsDraggingAbility)
        {
            Debug.Log("[PerkManager] Refusing clear: dragging ability.");
            return;
        }


        if (titleText != null)
            titleText.text = "";

        if (descriptionText != null)
            descriptionText.text = "";
    }

    public void RestoreSelectedPerk()
    {
        Debug.Log("[PerkManager] RestoreSelectedPerk");

        if (_selectedPerk != null && _selectedAbility != null)
            DisplayPerkDetails(_selectedPerk, _selectedAbility);
        else
            ClearPerkDetails();
    }

    public void NotifySelection(PerkButton selectedButton)
    {
        Debug.Log("[PerkManager] NotifySelection");

        foreach (PerkSlot slot in perkSlots)
        {
            if (slot.button != null)
                slot.button.SetSelected(slot.button == selectedButton);
        }
    }
}