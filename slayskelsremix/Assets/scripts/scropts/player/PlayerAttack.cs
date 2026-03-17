using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public charSO currentChar;
    public PlayerAim aimScript;
    public InputActionReference[] fireActions;
    public Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    void Update()
    {
        if (currentChar == null || currentChar.abilities == null) return;

        for (int i = 0; i < fireActions.Length; i++)
        {
            if (i >= currentChar.abilities.Length) break;
            HandleInput(fireActions[i], i);
        }
    }

    private void HandleInput(InputActionReference actionRef, int index)
    {
        if (actionRef == null || actionRef.action == null) return;

        Ability ability = currentChar.abilities[index];
        bool isHeld = actionRef.action.IsPressed();

        float cdTimestamp = GetCooldown(ability);
        bool isWeaponReady = Time.time >= cdTimestamp;

        // --- MELEE PATH ---
        if (ability is offensivemelee melee)
        {
            // Only try to swing if the global weapon cooldown is finished
            melee.Execute(transform, aimScript.anchor, isHeld && isWeaponReady);

            // Only apply recovery cooldown on release IF the weapon was ready to be used
            if (actionRef.action.WasReleasedThisFrame() && isWeaponReady)
            {
                ApplyCooldown(ability);
            }
        }
        // --- RANGED/OTHER PATH ---
        else
        {
            // Logic: Only fire if ready. This prevents the "reset cooldown" bug 
            // because the code inside never runs if isWeaponReady is false.
            if (isHeld && isWeaponReady)
            {
                ability.Execute(transform, aimScript.anchor, true);
                ApplyCooldown(ability);
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
        if (charsetter.Instance != null) charsetter.Instance.TriggerAbilityUsed(ability);
    }
}