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
            {
                HandleInput(fireActions[i], ability);
            }
        }
    }

    private void HandleInput(InputActionReference actionRef, Ability ability)
    {
        if (actionRef == null || actionRef.action == null) return;

        bool held = actionRef.action.IsPressed();

        float remaining = GetCooldownRemaining(ability);
        bool ready = remaining <= 0f;

        Debug.Log($"[ATTACK] {ability.abilityName} | Held={held} | Ready={ready} | Remaining={remaining:F2}");

        if (!held || !ready)
            return;

        bool comboFinished = ability.Execute(transform, aimScript.anchor, true);

        Debug.Log($"[EXECUTE] {ability.abilityName} | comboFinished={comboFinished}");

        if (ability is offensivemelee melee)
        {
            if (comboFinished)
            {
                float end = Time.time + ability.fireRate;
                cooldownEndTime[ability] = end;

                Debug.Log($"[COOLDOWN] FULL COMBO → fireRate={ability.fireRate}s ends at {end:F2}");
            }
            else
            {
                float end = Time.time + melee.swingFreq;
                cooldownEndTime[ability] = end;

                Debug.Log($"[COOLDOWN] SWING GAP → swingFreq={melee.swingFreq}s ends at {end:F2}");
            }
        }
        else
        {
            float end = Time.time + ability.fireRate;
            cooldownEndTime[ability] = end;

            Debug.Log($"[COOLDOWN] RANGED → fireRate={ability.fireRate}s ends at {end:F2}");
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