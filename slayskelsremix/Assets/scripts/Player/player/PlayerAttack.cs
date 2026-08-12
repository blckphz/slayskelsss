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

    private void Update()
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

    private void HandleInput(
        InputActionReference actionRef,
        Ability ability)
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

        // =====================================================
        // BLOCK INPUT WHEN MOUSE IS OVER UI
        // =====================================================

        if (IsPointerOverUI())
        {
            if (actionRef.action.IsPressed())
            {
                Debug.Log(
                    "[PlayerAttack] Attack blocked because pointer is over UI"
                );
            }

            return;
        }

        // =====================================================
        // BLOCK INPUT IN BUILD MODE
        // =====================================================

        if (BuildState.IsBuildMode)
        {
            if (actionRef.action.IsPressed())
            {
                Debug.Log(
                    "[PlayerAttack] Attack blocked because build mode is active"
                );
            }

            return;
        }

        if (!actionRef.action.IsPressed())
            return;

        // =====================================================
        // MELEE HAS ITS OWN COOLDOWN SYSTEM
        // =====================================================

        if (ability is not offensivemelee)
        {
            float remaining = GetCooldownRemaining(ability);

            if (remaining > 0f)
            {
                Debug.Log(
                    $"[PlayerAttack] Ability {ability.name} on cooldown: {remaining:F2}s"
                );

                return;
            }
        }

        // =====================================================
        // STAMINA CHECK
        // =====================================================

        if (movement != null &&
            !movement.HasEnoughStamina(ability.staminaUsed))
        {
            Debug.Log(
                $"[PlayerAttack] Not enough stamina for ability: {ability.name}"
            );

            return;
        }

        // =====================================================
        // EXECUTE ABILITY
        // =====================================================

        Debug.Log(
            $"[PlayerAttack] Executing ability: {ability.name}"
        );

        bool comboFinished = ability.Execute(
            transform,
            aimScript != null ? aimScript.anchor : null,
            true
        );

        Debug.Log(
            $"[PlayerAttack] Ability execute result: {comboFinished}"
        );

        // =====================================================
        // STAMINA
        // =====================================================

        if (comboFinished && movement != null)
        {
            movement.TryUseStamina(ability.staminaUsed);

            Debug.Log(
                $"[PlayerAttack] Used stamina: {ability.staminaUsed}"
            );
        }

        // =====================================================
        // NON-MELEE COOLDOWN
        //
        // Melee abilities handle their own cooldown.
        // =====================================================

        if (ability is not offensivemelee)
        {
            cooldownEndTime[ability] =
                Time.time + ability.fireRate;

            Debug.Log(
                $"[PlayerAttack] Cooldown applied: {ability.fireRate}s"
            );
        }

        // =====================================================
        // COSMETICS
        // =====================================================

        TriggerCosmetics(ability);
    }

    public float GetCooldownRemaining(Ability ability)
    {
        if (ability == null)
            return 0f;

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

    private void TriggerCosmetics(Ability ability)
    {
        if (ability == null)
            return;

        Debug.Log(
            $"[PlayerAttack] Triggering cosmetics for: {ability.name}"
        );

        if (CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(
                screenshakeIntensity,
                screenshakeDuration
            );

            Debug.Log(
                "[PlayerAttack] Camera shake triggered"
            );
        }

        if (charsetter.Instance != null)
        {
            charsetter.Instance.TriggerAbilityUsed(ability);

            Debug.Log(
                "[PlayerAttack] Charsetter animation triggered"
            );
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