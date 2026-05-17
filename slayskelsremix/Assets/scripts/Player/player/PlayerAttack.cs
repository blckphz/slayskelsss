using System.Collections; // 🔥 Added for IEnumerator
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

        // 🔥 CHECK 1: Respect the ScriptableObject's cooldown flag
        if (ability.isOnCooldown) return;

        bool isHeld = actionRef.action.IsPressed();
        float cdTimestamp = GetCooldown(ability);
        bool isWeaponReady = Time.time >= cdTimestamp;

        if (ability is offensivemelee melee)
        {
            bool wasSwingExecuted = melee.Execute(transform, aimScript.anchor, isHeld && isWeaponReady);

            bool reachedComboEnd = (melee.maxSwings > 0 && typeof(offensivemelee)
                .GetField("swingIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(melee) is int index && index == 0);

            if (isWeaponReady && (reachedComboEnd || actionRef.action.WasReleasedThisFrame()))
            {
                ApplyCooldown(ability);
                melee.ResetMeleeState();
            }
        }
        else
        {
            if (isHeld && isWeaponReady)
            {
                bool success = ability.Execute(transform, aimScript.anchor, true);

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
        abilityCooldowns[ability] = Time.time + ability.fireRate;

        // 🔥 CHECK 2: Start the routine to handle the bool flag flip
        StartCoroutine(AbilityCooldownRoutine(ability, ability.fireRate));

        // --- SCREEN SHAKE LOGIC ---
        if (CameraShaker.Instance != null && screenshakeIntensity > 0)
        {
            CameraShaker.Instance.Shake(screenshakeIntensity, screenshakeDuration);
        }

        if (charsetter.Instance != null)
            charsetter.Instance.TriggerAbilityUsed(ability);
    }

    // 🔥 New Coroutine to safely reset the ScriptableObject's state over time
    private IEnumerator AbilityCooldownRoutine(Ability ability, float duration)
    {
        ability.isOnCooldown = true;
        yield return new WaitForSeconds(duration);
        ability.isOnCooldown = false;
    }
}