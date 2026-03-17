using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrenadeInput : MonoBehaviour
{
    [Header("References")]
    public grenadeSO grenadeAbility;
    public Transform throwPoint;   // Where the grenade spawns
    public Transform targetAnchor; // Where the player is aiming

    [Header("Input Action")]
    [SerializeField] private InputActionReference grenadeAction;

    [Header("Cooldown Settings")]
    public float throwCooldown = 1.5f;
    private float lastThrowTime;

    private void OnEnable()
    {
        if (grenadeAction == null)
        {
            Debug.LogError($"[Input] Action Reference is missing on {gameObject.name}!");
            return;
        }

        grenadeAction.action.started += OnGrenadePressed;
        grenadeAction.action.Enable();
    }

    private void OnDisable()
    {
        if (grenadeAction != null)
        {
            grenadeAction.action.started -= OnGrenadePressed;
            grenadeAction.action.Disable();
        }
    }

    private void OnGrenadePressed(InputAction.CallbackContext context)
    {
        HandleGrenadeAction();
    }

    void HandleGrenadeAction()
    {
        // 1. If a grenade is currently in the air, DETONATE IT
        if (grenadeBehav.ActiveGrenade != null)
        {
            Debug.Log("[Input] Manual Detonation Triggered!");
            grenadeBehav.ActiveGrenade.ManualExplode();
        }
        // 2. Otherwise, check if we can THROW a new one
        else
        {
            // Cooldown check
            if (Time.time < lastThrowTime + throwCooldown)
            {
                Debug.Log("Grenade is still on cooldown.");
                return;
            }

            if (grenadeAbility != null)
            {
                Debug.Log("[Input] Throwing Grenade.");

                lastThrowTime = Time.time;
                grenadeBehav.ActiveGrenadeSO = grenadeAbility;

                Transform finalTarget = targetAnchor != null ? targetAnchor : throwPoint;

                // FIXED: Added 'true' as the third argument to match the new Ability signature
                grenadeAbility.Execute(throwPoint, finalTarget, true);
            }
        }
    }
}