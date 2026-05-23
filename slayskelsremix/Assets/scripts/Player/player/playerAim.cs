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

    [Header("Aim Settings")]
    public float anchorMaxDist = 3f;
    public float anchorSmooth = 15f;
    public float lockOnRadius = 10f;

    [Header("Light Settings")]
    public float lightMaxRange = 8f;
    public float rotationOffset = -90f;
    public float lightMinRadius = 0.5f;

    [Header("Beam")]
    [Range(0, 360)] public float beamAngle = 35f;

    private Camera cam;
    private Transform lockedEnemy;
    private bool isLockedOn;
    private bool isInventoryOpen;

    private Vector3 fullDir;

    public Vector3 AimDirection => fullDir;

    private void Awake()
    {
        cam = Camera.main;

        if (anchor != null && anchor.parent == player)
            anchor.SetParent(null);
    }

    private void OnEnable()
    {
        if (lockOnAction != null)
        {
            lockOnAction.action.performed += OnLockOnPressed;
            lockOnAction.action.Enable();
        }

        if (lookAction != null)
            lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lockOnAction != null)
        {
            lockOnAction.action.performed -= OnLockOnPressed;
            lockOnAction.action.Disable();
        }
    }

    public void SetInventoryState(bool open) => isInventoryOpen = open;

    private void OnLockOnPressed(InputAction.CallbackContext ctx)
    {
        if (isInventoryOpen) return;

        // ❌ BLOCK LOCKON IN BUILD MODE
        if (BuildState.IsBuildMode)
            return;

        if (isLockedOn)
        {
            isLockedOn = false;
            lockedEnemy = null;
        }
        else
        {
            lockedEnemy = FindClosestEnemy();
            isLockedOn = lockedEnemy != null;
        }
    }

    private Transform FindClosestEnemy()
    {
        enemyHealth[] enemies = Object.FindObjectsByType<enemyHealth>(FindObjectsSortMode.None);

        Transform closest = null;
        float minDist = lockOnRadius;

        foreach (enemyHealth e in enemies)
        {
            float dist = Vector2.Distance(player.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e.transform;
            }
        }

        return closest;
    }

    private void LateUpdate()
    {
        if (player == null || cam == null || lookAction == null)
            return;

        // =========================
        // INPUT → DIRECTION
        // =========================
        Vector2 input = lookAction.action.ReadValue<Vector2>();
        Vector3 mouseWorldPos;

        bool isMouse = lookAction.action.activeControl?.device is Pointer;

        if (isMouse)
        {
            float z = Mathf.Abs(cam.transform.position.z);
            mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(input.x, input.y, z));
        }
        else
        {
            mouseWorldPos = player.position + (Vector3)input * 5f;
        }

        fullDir = mouseWorldPos - player.position;
        fullDir.z = 0f;

        // =========================
        // ❌ BUILD MODE BLOCK
        // =========================
        if (BuildState.IsBuildMode)
        {
            if (anchor != null)
                anchor.position = Vector3.Lerp(anchor.position, player.position, anchorSmooth * Time.deltaTime);

            if (spotlight != null)
            {
                spotlight.transform.position = player.position;
                spotlight.pointLightOuterRadius = 0f;
            }

            return;
        }

        // =========================
        // ANCHOR / AIM
        // =========================
        if (anchor != null)
        {
            Vector3 target;

            if (isLockedOn && lockedEnemy != null)
            {
                target = lockedEnemy.position;

                // auto unlock if too far
                if (Vector2.Distance(player.position, lockedEnemy.position) > lockOnRadius + 2f)
                {
                    isLockedOn = false;
                    lockedEnemy = null;
                }
            }
            else
            {
                Vector3 dir = fullDir;

                if (dir.magnitude > anchorMaxDist)
                    dir = dir.normalized * anchorMaxDist;

                target = player.position + dir;
            }

            anchor.position = Vector3.Lerp(anchor.position, target, anchorSmooth * Time.deltaTime);
        }

        // =========================
        // LIGHT
        // =========================
        if (spotlight != null)
        {
            spotlight.transform.position = player.position;

            Vector3 lookDir = (isLockedOn && lockedEnemy != null)
                ? (lockedEnemy.position - player.position)
                : fullDir;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            spotlight.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

            float len = Mathf.Clamp(lookDir.magnitude, lightMinRadius, lightMaxRange);

            spotlight.pointLightOuterRadius = len;
            spotlight.pointLightInnerRadius = len * 0.1f;
            spotlight.pointLightOuterAngle = beamAngle;
            spotlight.pointLightInnerAngle = beamAngle * 0.5f;
        }
    }
}