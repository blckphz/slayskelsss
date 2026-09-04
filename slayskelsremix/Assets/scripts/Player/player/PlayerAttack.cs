using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerAttack : MonoBehaviour
{
    public PlayerAim aimScript;

    public InputActionReference[] fireActions;

    private Dictionary<Ability, float> cooldownEndTime =
        new Dictionary<Ability, float>();

    public float screenshakeIntensity = 0.5f;
    public float screenshakeDuration = 0.2f;

    private PlayerMovement movement;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }


    private void Start()
    {
        cooldownEndTime.Clear();
    }


    private void OnEnable()
    {
        cooldownEndTime.Clear();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (AbilityLoadout.Instance == null)
        {
            Debug.LogWarning(
                "[PlayerAttack] AbilityLoadout.Instance is NULL"
            );

            return;
        }


        for (int i = 0; i < fireActions.Length; i++)
        {
            if (i >=
                AbilityLoadout.Instance.equippedAbilities.Length)
            {
                break;
            }


            Ability ability =
                AbilityLoadout.Instance.GetAbility(i);


            if (ability != null)
            {
                HandleInput(
                    fireActions[i],
                    ability
                );
            }
        }
    }


    // =========================================================
    // INPUT
    // =========================================================

    private void HandleInput(
        InputActionReference actionRef,
        Ability ability)
    {
        if (actionRef == null)
        {
            Debug.LogWarning(
                "[PlayerAttack] Action Reference is NULL"
            );

            return;
        }


        if (actionRef.action == null)
        {
            Debug.LogWarning(
                "[PlayerAttack] Input Action is NULL"
            );

            return;
        }


        // =====================================================
        // UI
        // =====================================================

        if (IsPointerOverUI())
        {
            return;
        }


        // =====================================================
        // BUILD MODE
        // =====================================================

        if (BuildState.IsBuildMode)
        {
            return;
        }


        // =====================================================
        // INPUT
        // =====================================================

        if (!actionRef.action.IsPressed())
        {
            return;
        }


        // =====================================================
        // NON-MELEE COOLDOWN
        // =====================================================

        if (ability is not offensivemelee)
        {
            float remaining =
                GetCooldownRemaining(ability);

            if (remaining > 0f)
            {
                Debug.Log(
                    $"[PlayerAttack] " +
                    $"{ability.name} on cooldown: " +
                    $"{remaining:F2}s"
                );

                return;
            }
        }


        // =====================================================
        // STAMINA
        // =====================================================

        if (movement != null &&
            !movement.HasEnoughStamina(
                ability.staminaUsed))
        {
            Debug.Log(
                $"[PlayerAttack] Not enough stamina: " +
                $"{ability.name}"
            );

            return;
        }


        // =====================================================
        // EXECUTE
        // =====================================================

        Debug.Log(
            $"[PlayerAttack] Executing: {ability.name}"
        );


        bool comboFinished =
            ability.Execute(
                transform,
                aimScript != null
                    ? aimScript.anchor
                    : null,
                true
            );


        Debug.Log(
            $"[PlayerAttack] Execute result: " +
            $"{comboFinished}"
        );


        // =====================================================
        // STAMINA
        // =====================================================

        if (comboFinished &&
            movement != null)
        {
            movement.TryUseStamina(
                ability.staminaUsed
            );

            Debug.Log(
                $"[PlayerAttack] Used stamina: " +
                $"{ability.staminaUsed}"
            );
        }


        // =====================================================
        // NON-MELEE COOLDOWN
        // =====================================================

        if (ability is not offensivemelee)
        {
            cooldownEndTime[ability] =
                Time.time + ability.fireRate;

            Debug.Log(
                $"[PlayerAttack] Cooldown applied: " +
                $"{ability.fireRate:F2}s"
            );
        }


        // =====================================================
        // COSMETICS
        // =====================================================

        TriggerCosmetics(ability);
    }


    // =========================================================
    // COOLDOWN
    // =========================================================

    public float GetCooldownRemaining(
        Ability ability)
    {
        if (ability == null)
        {
            return 0f;
        }


        if (!cooldownEndTime.TryGetValue(
                ability,
                out float cooldownEnd))
        {
            return 0f;
        }


        return Mathf.Max(
            0f,
            cooldownEnd - Time.time
        );
    }


    // =========================================================
    // COSMETICS
    // =========================================================

    private void TriggerCosmetics(
        Ability ability)
    {
        if (ability == null)
        {
            return;
        }


        if (CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(
                screenshakeIntensity,
                screenshakeDuration
            );
        }


        if (charsetter.Instance != null)
        {
            charsetter.Instance.TriggerAbilityUsed(
                ability
            );
        }
    }


    // =========================================================
    // UI CHECK
    // =========================================================

    private bool IsPointerOverUI()
    {
        return
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();
    }
}