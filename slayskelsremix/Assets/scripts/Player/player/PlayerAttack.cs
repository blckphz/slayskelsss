using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public PlayerAim aimScript;
    public InputActionReference[] fireActions;

    private Dictionary<Ability, float> cooldownEndTime = new Dictionary<Ability, float>();

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
                HandleInput(fireActions[i], ability);
        }
    }

    private void HandleInput(InputActionReference actionRef, Ability ability)
    {
        if (actionRef == null || actionRef.action == null) return;

        // ❌ BLOCK BUILD MODE
        if (BuildState.IsBuildMode)
            return;

        bool held = actionRef.action.IsPressed();

        float remaining = GetCooldownRemaining(ability);
        bool ready = remaining <= 0f;

        if (!held || !ready)
            return;

        bool comboFinished = ability.Execute(transform, aimScript.anchor, true);

        if (ability is offensivemelee melee)
        {
            float end = Time.time + (comboFinished ? ability.fireRate : melee.swingFreq);
            cooldownEndTime[ability] = end;
        }
        else
        {
            cooldownEndTime[ability] = Time.time + ability.fireRate;
        }

        TriggerCosmetics(ability);
    }

    public float GetCooldownRemaining(Ability ability)
    {
        if (!cooldownEndTime.ContainsKey(ability))
            return 0f;

        return Mathf.Max(0f, cooldownEndTime[ability] - Time.time);
    }

    private void TriggerCosmetics(Ability ability)
    {
        if (CameraShaker.Instance != null)
            CameraShaker.Instance.Shake(screenshakeIntensity, screenshakeDuration);

        if (charsetter.Instance != null)
            charsetter.Instance.TriggerAbilityUsed(ability);
    }
}