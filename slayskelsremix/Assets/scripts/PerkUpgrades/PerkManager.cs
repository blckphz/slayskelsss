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
            if (slot == null || slot.perk == null || slot.button == null)
                continue;

            slot.perk.LoadLevel();

            // 🔥 AUTO-APPLY PASSIVES ON LOAD
            if (slot.perk is PassivePerkSO passive && slot.perk.Level > 0)
            {
                passive.ApplyPassive();
            }

            slot.button.SetupButton(this, slot.targetAbility, slot.perk);
        }
    }

    public void SelectPerk(AbilityUpgradeSO perkAsset, Ability ability)
    {
        Debug.Log($"[PerkManager] SelectPerk: {perkAsset?.name}");

        if (perkAsset == null)
        {
            Debug.LogWarning("[PerkManager] SelectPerk failed: perk is null.");
            return;
        }

        _selectedPerk = perkAsset;
        _selectedAbility = ability;

        if (ability != null)
            DisplayPerkDetails(perkAsset, ability);
        else
            DisplayPassivePerkDetails(perkAsset);
    }

    private void DisplayPassivePerkDetails(AbilityUpgradeSO perkAsset)
    {
        if (titleText == null || descriptionText == null)
            return;

        var info = perkAsset.GetDisplayStrings();

        titleText.text = info.displayName;

        descriptionText.text =
            $"<b>Passive Perk</b>\n" +
            $"Level: {perkAsset.Level}/{perkAsset.MaxLevel}\n\n" +
            info.displayDesc;
    }

    public void ConfirmUpgrade()
    {
        Debug.Log("[PerkManager] ConfirmUpgrade");

        if (_selectedPerk == null)
            return;

        if (_selectedPerk.Level >= _selectedPerk.MaxLevel)
        {
            Debug.Log("[PerkManager] Perk already maxed.");
            return;
        }

        _selectedPerk.Level++;
        _selectedPerk.SaveLevel();

        // 🔥 PASSIVE AUTO APPLY ON EVERY LEVEL
        if (_selectedPerk is PassivePerkSO passive)
        {
            Debug.Log($"[PerkManager] Updating passive: {passive.name}");

            passive.ApplyPassive();
        }
        else
        {
            if (_selectedAbility != null)
                _selectedPerk.Apply(_selectedAbility);
        }

        if (UIShaker.Instance != null)
            UIShaker.Instance.ShakeUI(0.2f, 20f);

        if (_selectedPerk is PassivePerkSO)
            DisplayPassivePerkDetails(_selectedPerk);
        else if (_selectedAbility != null)
            DisplayPerkDetails(_selectedPerk, _selectedAbility);
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

        if (_selectedPerk == null)
        {
            ClearPerkDetails();
            return;
        }

        if (_selectedAbility != null)
            DisplayPerkDetails(_selectedPerk, _selectedAbility);
        else
            DisplayPassivePerkDetails(_selectedPerk);
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