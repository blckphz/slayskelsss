using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public PlayerAim aimScript;
    public InputActionReference[] fireActions;
    public Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();
    public float screenshakeIntensity = 0.5f;
    public float screenshakeDuration = 0.2f;


    void Update()
    {
        if (AbilityLoadout.Instance == null) return;

        // Loop through the 4 slots
        for (int i = 0; i < fireActions.Length; i++)
        {
            if (i >= AbilityLoadout.Instance.equippedAbilities.Length) break;

            Ability ability = AbilityLoadout.Instance.GetAbility(i);
            if (ability != null)
            {
                HandleInput(fireActions[i], ability);
            }
        }
    }

    private void HandleInput(InputActionReference actionRef, Ability ability)
    {
        if (actionRef == null || actionRef.action == null) return;

        bool isHeld = actionRef.action.IsPressed();
        float cdTimestamp = GetCooldown(ability);
        bool isWeaponReady = Time.time >= cdTimestamp;

        if (ability is offensivemelee melee)
        {
            // Execute returns true if the combo is finished or a specific swing logic triggers completion
            bool comboFinished = melee.Execute(transform, aimScript.anchor, isHeld && isWeaponReady);

            if (isWeaponReady && (comboFinished || actionRef.action.WasReleasedThisFrame()))
            {
                ApplyCooldown(ability);
            }
        }
        else
        {
            if (isHeld && isWeaponReady)
            {
                // Execute the ability
                bool success = ability.Execute(transform, aimScript.anchor, true);

                // If the ability returns true (it actually fired), start cooldown and shake
                if (success)
                {
                    ApplyCooldown(ability);
                }
            }
        }
    }

    private float GetCooldown(Ability ability)
    {
        if (!abilityCooldowns.ContainsKey(ability)) abilityCooldowns[ability] = 0f;
        return abilityCooldowns[ability];
    }

    private void ApplyCooldown(Ability ability)
    {
        // Set the cooldown timer
        abilityCooldowns[ability] = Time.time + ability.fireRate;

        // --- SCREEN SHAKE LOGIC ---
        // Trigger the shake based on the variables in your Ability ScriptableObject
        if (CameraShaker.Instance != null && screenshakeIntensity > 0)
        {
            CameraShaker.Instance.Shake(screenshakeIntensity, screenshakeDuration);
        }

        // Trigger character animations/effects
        if (charsetter.Instance != null)
            charsetter.Instance.TriggerAbilityUsed(ability);
    }
}