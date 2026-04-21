using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerAim : MonoBehaviour
{
    [Header("Core References")]
    public Transform player;
    public Transform anchor;
    public Light2D spotlight;

    [Header("Input")]
    public InputActionReference lookAction;
    public InputActionReference lockOnAction;

    [Header("Aim Settings (Camera)")]
    public float anchorMaxDist = 3f;
    public float anchorSmooth = 15f;
    public float lockOnRadius = 10f;

    [Header("Light Settings (Visual)")]
    public float lightMaxRange = 8f;
    public float rotationOffset = -90f;
    public float lightMinRadius = 0.5f;

    [Header("Light Appearance")]
    [Range(0, 360)] public float beamAngle = 35f;

    private Camera cam;
    private Transform lockedEnemy;
    private bool isLockedOn = false;
    private bool isInventoryOpen = false;

    private void Awake()
    {
        cam = Camera.main;
        // Detach anchor from player to allow smooth following without jitter
        if (anchor != null && anchor.parent == player) anchor.SetParent(null);
    }

    private void OnEnable()
    {
        if (lockOnAction != null)
        {
            lockOnAction.action.performed += OnLockOnPressed;
            lockOnAction.action.Enable();
        }

        if (lookAction != null) lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lockOnAction != null)
        {
            lockOnAction.action.performed -= OnLockOnPressed;
            lockOnAction.action.Disable();
        }
    }

    /// <summary>
    /// Now only tracks state. Movement logic is no longer restricted by this.
    /// </summary>
    public void SetInventoryState(bool isOpen)
    {
        isInventoryOpen = isOpen;
    }

    private void OnLockOnPressed(InputAction.CallbackContext context)
    {
        // We still block toggling lock-on targets while in inventory 
        // to prevent accidental targeting while clicking UI.
        if (isInventoryOpen) return;

        if (isLockedOn)
        {
            isLockedOn = false;
            lockedEnemy = null;
        }
        else
        {
            lockedEnemy = FindClosestEnemy();
            if (lockedEnemy != null)
            {
                isLockedOn = true;
            }
        }
    }

    private Transform FindClosestEnemy()
    {
        enemyHealth[] enemies = Object.FindObjectsByType<enemyHealth>(FindObjectsSortMode.None);
        Transform closest = null;
        float minDist = lockOnRadius;

        foreach (enemyHealth enemy in enemies)
        {
            float dist = Vector2.Distance(player.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.transform;
            }
        }
        return closest;
    }

    void LateUpdate()
    {
        if (player == null || cam == null || lookAction == null)
            return;

        // 1. GET RAW INPUT POSITION
        Vector2 input = lookAction.action.ReadValue<Vector2>();
        Vector3 mouseWorldPos;
        bool isMouse = lookAction.action.activeControl?.device is Pointer;

        if (isMouse)
        {
            float distanceToPlane = Mathf.Abs(cam.transform.position.z);
            mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(input.x, input.y, distanceToPlane));
        }
        else
        {
            mouseWorldPos = player.position + (Vector3)input * 5f;
        }

        Vector3 fullDir = mouseWorldPos - player.position;
        fullDir.z = 0;

        // 2. ANCHOR LOGIC (Camera/Aim)
        // This now runs normally even if isInventoryOpen is true.
        if (anchor != null)
        {
            Vector3 targetPos;

            if (isLockedOn && lockedEnemy != null)
            {
                targetPos = lockedEnemy.position;

                // Auto-Unlock if enemy dies or goes too far
                if (Vector2.Distance(player.position, lockedEnemy.position) > lockOnRadius + 2f)
                {
                    isLockedOn = false;
                    lockedEnemy = null;
                }
            }
            else
            {
                Vector3 anchorDir = fullDir;
                if (anchorDir.magnitude > anchorMaxDist)
                    anchorDir = anchorDir.normalized * anchorMaxDist;

                targetPos = player.position + anchorDir;
            }

            // Move the anchor smoothly toward the target
            anchor.position = Vector3.Lerp(anchor.position, targetPos, anchorSmooth * Time.deltaTime);
        }

        // 3. LIGHT LOGIC
        if (spotlight != null)
        {
            spotlight.transform.position = player.position;

            // Follow the aim/enemy regardless of UI state
            Vector3 lookDir = (isLockedOn && lockedEnemy != null) ? (lockedEnemy.position - player.position) : fullDir;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            spotlight.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

            float lightLength = Mathf.Clamp(lookDir.magnitude, lightMinRadius, lightMaxRange);
            spotlight.pointLightOuterRadius = lightLength;
            spotlight.pointLightInnerRadius = lightLength * 0.1f;
            spotlight.pointLightOuterAngle = beamAngle;
            spotlight.pointLightInnerAngle = beamAngle * 0.5f;
        }
    }
}