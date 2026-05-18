using System.Collections;
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

        // CRITICAL: Block input if the big fireRate cooldown asset flag is active
        if (ability.isOnCooldown) return;

        bool isHeld = actionRef.action.IsPressed();
        float cdTimestamp = GetCooldown(ability);
        bool isWeaponReady = Time.time >= cdTimestamp;

        if (isHeld && isWeaponReady)
        {
            // Execute returns true ONLY when the combo completely finishes its last hit
            bool comboFinished = ability.Execute(transform, aimScript.anchor, true);

            if (ability is offensivemelee melee)
            {
                if (comboFinished)
                {
                    // Combo Finished: Apply the BIG cooldown (fireRate)
                    abilityCooldowns[ability] = Time.time + ability.fireRate;
                    StartCoroutine(BigCooldownRoutine(ability, ability.fireRate));
                }
                else
                {
                    // Mid-Combo: Just space out the next swing locally using swingFreq
                    abilityCooldowns[ability] = Time.time + melee.swingFreq;
                }
            }
            else
            {
                // Standard ranged/berry actions use the traditional fireRate structure instantly
                abilityCooldowns[ability] = Time.time + ability.fireRate;
                StartCoroutine(BigCooldownRoutine(ability, ability.fireRate));
            }

            // Trigger visual cosmetics per swing
            TriggerCosmetics(ability);
        }

        // Reset tracking if button is released mid-combo execution
        if (actionRef.action.WasReleasedThisFrame() && ability is offensivemelee manualMelee)
        {
            manualMelee.ResetMeleeState();
        }
    }

    private float GetCooldown(Ability ability)
    {
        if (!abilityCooldowns.ContainsKey(ability)) abilityCooldowns[ability] = 0f;
        return abilityCooldowns[ability];
    }

    private void TriggerCosmetics(Ability ability)
    {
        if (CameraShaker.Instance != null && screenshakeIntensity > 0)
        {
            CameraShaker.Instance.Shake(screenshakeIntensity, screenshakeDuration);
        }

        if (charsetter.Instance != null)
            charsetter.Instance.TriggerAbilityUsed(ability);
    }

    // This handles the scriptable object's big asset cooldown lock
    private IEnumerator BigCooldownRoutine(Ability ability, float duration)
    {
        ability.isOnCooldown = true;
        yield return new WaitForSeconds(duration);
        ability.isOnCooldown = false;
    }
}