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
            grenadeBehav.ActiveGrenade.ManualExplode();
        }
    }
}