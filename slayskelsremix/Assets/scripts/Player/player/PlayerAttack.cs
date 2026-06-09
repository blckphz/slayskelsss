using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
        {
            Debug.LogWarning("[PlayerAttack] AbilityLoadout.Instance is NULL");
            return;
        }

        for (int i = 0; i < fireActions.Length; i++)
        {
            if (i >= AbilityLoadout.Instance.equippedAbilities.Length)
                break;

            Ability ability = AbilityLoadout.Instance.GetAbility(i);

            if (ability != null)
            {
                HandleInput(fireActions[i], ability);
            }
        }
    }

    private void HandleInput(InputActionReference actionRef, Ability ability)
    {
        if (actionRef == null)
        {
            Debug.LogWarning("[PlayerAttack] Action Reference is NULL");
            return;
        }

        if (actionRef.action == null)
        {
            Debug.LogWarning("[PlayerAttack] Input Action is NULL");
            return;
        }

        // BLOCK INPUT WHEN MOUSE OVER UI
        if (IsPointerOverUI())
        {
            if (actionRef.action.IsPressed())
            {
                Debug.Log("[PlayerAttack] Attack blocked because pointer is over UI");
            }

            return;
        }

        if (BuildState.IsBuildMode)
        {
            if (actionRef.action.IsPressed())
            {
                Debug.Log("[PlayerAttack] Attack blocked because build mode is active");
            }

            return;
        }

        if (!actionRef.action.IsPressed())
            return;

        float remaining = GetCooldownRemaining(ability);

        if (remaining > 0f)
        {
            Debug.Log($"[PlayerAttack] Ability {ability.name} on cooldown: {remaining:F2}s");
            return;
        }

        if (movement != null && !movement.HasEnoughStamina(ability.staminaUsed))
        {
            Debug.Log($"[PlayerAttack] Not enough stamina for ability: {ability.name}");
            return;
        }

        Debug.Log($"[PlayerAttack] Executing ability: {ability.name}");

        bool comboFinished = ability.Execute(transform, aimScript.anchor, true);

        Debug.Log($"[PlayerAttack] Ability execute result: {comboFinished}");

        if (comboFinished && movement != null)
        {
            movement.TryUseStamina(ability.staminaUsed);

            Debug.Log($"[PlayerAttack] Used stamina: {ability.staminaUsed}");
        }

        cooldownEndTime[ability] =
            Time.time + ability.fireRate;

        Debug.Log($"[PlayerAttack] Cooldown applied: {ability.fireRate}s");

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
        Debug.Log($"[PlayerAttack] Triggering cosmetics for: {ability.name}");

        if (CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(
                screenshakeIntensity,
                screenshakeDuration
            );

            Debug.Log("[PlayerAttack] Camera shake triggered");
        }

        if (charsetter.Instance != null)
        {
            charsetter.Instance.TriggerAbilityUsed(ability);

            Debug.Log("[PlayerAttack] Charsetter animation triggered");
        }
    }

    private bool IsPointerOverUI()
    {
        bool overUI =
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();


        return overUI;
    }
}