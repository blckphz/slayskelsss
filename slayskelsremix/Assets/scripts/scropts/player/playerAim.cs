using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform anchor; // This is what the camera should likely follow/track

    [Header("Input")]
    public InputActionReference lookAction;

    [Header("Settings")]
    public float maxDistance = 3f;
    public float smoothSpeed = 20f; // Higher speed feels better with Cinemachine

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        // Optimization: Ensure the anchor is NOT a child of the player 
        // to prevent "double-flipping" when the player scales.
        if (anchor.parent == player)
        {
            anchor.SetParent(null);
            Debug.LogWarning("Anchor was a child of Player. Unparented to prevent scaling issues.");
        }
    }

    void LateUpdate() // Use LateUpdate when working with Cinemachine
    {
        if (player == null || anchor == null || cam == null) return;

        Vector2 input = lookAction.action.ReadValue<Vector2>();
        Vector3 targetWorldPos;

        // Determine if using Mouse or Controller
        bool isMouse = lookAction.action.activeControl?.device is Pointer;

        if (isMouse)
        {
            // IMPORTANT: ScreenToWorldPoint needs the distance from the camera to the 2D plane
            float distanceToPlane = Mathf.Abs(cam.transform.position.z);
            targetWorldPos = cam.ScreenToWorldPoint(new Vector3(input.x, input.y, distanceToPlane));
        }
        else
        {
            targetWorldPos = player.position + (Vector3)input * 5f;
        }

        // Clamp the distance so the 'aim' doesn't go off-screen
        Vector3 dir = targetWorldPos - player.position;
        dir.z = 0; // Lock to 2D

        if (dir.magnitude > maxDistance)
            dir = dir.normalized * maxDistance;

        // Smoothly move the anchor
        anchor.position = Vector3.Lerp(anchor.position, player.position + dir, smoothSpeed * Time.deltaTime);

        // Debug Visual
        Debug.DrawLine(player.position, anchor.position, Color.green);
    }
}