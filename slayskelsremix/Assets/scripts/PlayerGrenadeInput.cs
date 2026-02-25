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
        Debug.Log("[Input] Grenade Key Pressed.");
        HandleGrenadeAction();
    }

    void HandleGrenadeAction()
    {
        // 1. Check if a grenade already exists in the world
        if (grenadeBehav.ActiveGrenade != null)
        {
            Debug.Log("[Input] Found Active Grenade. Triggering Manual Detonation.");
            grenadeBehav.ActiveGrenade.ManualExplode();
        }
        // 2. If no grenade exists, throw a new one
        else
        {
            if (grenadeAbility != null)
            {
                Debug.Log("[Input] No active grenade. Executing throw via SO.");
                // Ensure the SO knows which prefab to track
                grenadeBehav.ActiveGrenadeSO = grenadeAbility;
                grenadeAbility.Execute(throwPoint, targetAnchor);
            }
            else
            {
                Debug.LogWarning("[Input] No grenadeSO assigned to PlayerGrenadeInput!");
            }
        }
    }
}