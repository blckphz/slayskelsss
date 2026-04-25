using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem; // 1. Added for Input System support

public class PerkManager : MonoBehaviour
{
    [Header("UI Panel Toggle")]
    public GameObject upgradePanel; // The GameObject you want to open/close
    public InputActionProperty toggleAction; // The "ToggleUpgrade" action
    public perkTooltip tooltip;

    [Header("Text Display")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Settings")]
    public AbilityDatabase abilityDatabase;

    [Header("Master Perk List")]
    public List<PerkSlot> perkSlots = new List<PerkSlot>();



    void Awake()
    {
        if (abilityDatabase == null)
        {
            Debug.LogError("<color=red>[PerkManager]</color> AbilityDatabase is missing!");
            return;
        }

        InitializeAllSlots();

        // Ensure the panel starts in the correct state (usually closed)
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    // 2. Enable/Disable the input action
    private void OnEnable() => toggleAction.action.Enable();
    private void OnDisable() => toggleAction.action.Disable();

    void Update()
    {
        // 3. Check for the toggle input
        if (toggleAction.action.WasPressedThisFrame())
        {
            ToggleUpgradeUI();
        }
    }

    public void ToggleUpgradeUI()
    {
        if (upgradePanel != null)
        {
            bool isActive = upgradePanel.activeSelf;
            upgradePanel.SetActive(!isActive);
            if (isActive && tooltip != null) tooltip.HideTooltip();

        }
    }

    void InitializeAllSlots()
    {
        foreach (PerkSlot slot in perkSlots)
        {
            if (slot.button == null || slot.perk == null || slot.uiComponent == null)
            {
                Debug.LogWarning($"<color=yellow>[PerkManager]</color> Slot '{slot.targetAbility}' is missing references!");
                continue;
            }

            slot.perk.LoadLevel();
            slot.button.SetupButton(this, slot.uiComponent, slot.targetAbility, slot.perk);
        }

        if (perkSlots.Count > 0) perkSlots[0].uiComponent.ClearInfo();
    }
}