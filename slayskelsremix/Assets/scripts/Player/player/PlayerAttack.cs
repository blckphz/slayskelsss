using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public PlayerAim aimScript;
    public InputActionReference[] fireActions;

    private Dictionary<Ability, float> cooldownEndTime = new();

    public float screenshakeIntensity = 0.5f;
    public float screenshakeDuration = 0.2f;

    private PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        cooldownEndTime.Clear();
    }

    void OnEnable()
    {
        cooldownEndTime.Clear();
    }

    void Update()
    {
        if (AbilityLoadout.Instance == null)
            return;

        for (int i = 0; i < fireActions.Length; i++)
        {
            if (i >= AbilityLoadout.Instance.equippedAbilities.Length)
                break;

            Ability ability = AbilityLoadout.Instance.GetAbility(i);

            if (ability != null)
                HandleInput(fireActions[i], ability);
        }
    }

    private void HandleInput(InputActionReference actionRef, Ability ability)
    {
        if (actionRef == null || actionRef.action == null)
            return;

        if (BuildState.IsBuildMode)
            return;

        if (!actionRef.action.IsPressed())
            return;

        float remaining = GetCooldownRemaining(ability);
        if (remaining > 0f)
            return;

        if (movement != null && !movement.HasEnoughStamina(ability.staminaUsed))
            return;

        bool comboFinished = ability.Execute(transform, aimScript.anchor, true);

        if (comboFinished && movement != null)
            movement.TryUseStamina(ability.staminaUsed);

        cooldownEndTime[ability] =
            Time.time + ability.fireRate;

        TriggerCosmetics(ability);
    }

    public float GetCooldownRemaining(Ability ability)
    {
        if (!cooldownEndTime.TryGetValue(ability, out float t))
            return 0f;

        return Mathf.Max(0f, t - Time.time);
    }

    private void TriggerCosmetics(Ability ability)
    {
        if (CameraShaker.Instance != null)
            CameraShaker.Instance.Shake(screenshakeIntensity, screenshakeDuration);

        if (charsetter.Instance != null)
            charsetter.Instance.TriggerAbilityUsed(ability);
    }
}