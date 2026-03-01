using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerAim : MonoBehaviour
{
    [Header("Core References")]
    public Transform player;
    public Transform anchor;      // The Camera/Reticle Target
    public Light2D spotlight;     // The Visual Light

    [Header("Input")]
    public InputActionReference lookAction;

    [Header("Aim Settings (Camera)")]
    public float anchorMaxDist = 3f;   // How far the camera can "peek"
    public float anchorSmooth = 15f;

    [Header("Light Settings (Visual)")]
    public float lightMaxRange = 8f;   // How long the beam can grow
    public float rotationOffset = -90f;
    public float lightMinRadius = 0.5f;

    [Header("Light Appearance")]
    [Range(0, 360)] public float beamAngle = 35f;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        // Ensure anchor is independent
        if (anchor != null && anchor.parent == player) anchor.SetParent(null);
    }

    void LateUpdate()
    {
        if (player == null || cam == null) return;

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

        // 2. INDEPENDENT ANCHOR LOGIC (Camera/Aim)
        if (anchor != null)
        {
            Vector3 anchorDir = fullDir;
            if (anchorDir.magnitude > anchorMaxDist)
                anchorDir = anchorDir.normalized * anchorMaxDist;

            anchor.position = Vector3.Lerp(anchor.position, player.position + anchorDir, anchorSmooth * Time.deltaTime);
        }

        // 3. INDEPENDENT LIGHT LOGIC (Visuals)
        if (spotlight != null)
        {
            // Position: Always glued to player
            spotlight.transform.position = player.position;

            // Rotation: Always points at raw mouse (ignores anchor clamp)
            float angle = Mathf.Atan2(fullDir.y, fullDir.x) * Mathf.Rad2Deg;
            spotlight.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

            // Length: Its own independent clamp
            float lightLength = Mathf.Clamp(fullDir.magnitude, lightMinRadius, lightMaxRange);
            spotlight.pointLightOuterRadius = lightLength;
            spotlight.pointLightInnerRadius = lightLength * 0.1f;

            // Shape: Hardcoded or adjustable via inspector
            spotlight.pointLightOuterAngle = beamAngle;
            spotlight.pointLightInnerAngle = beamAngle * 0.5f;
        }
    }
}