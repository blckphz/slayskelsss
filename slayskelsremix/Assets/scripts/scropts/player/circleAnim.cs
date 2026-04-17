using UnityEngine;

public class CircleAnim : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerScript;

    [Header("Animation Settings")]
    public float pulseSpeed = 12f;
    public float pulseMagnitude = 0.15f;
    public float returnSpeed = 10f; // Higher = faster reset

    private Vector3 originalScale;

    void Start()
    {
        // Save the exact scale from your Inspector
        originalScale = transform.localScale;

        if (playerScript == null)
            Debug.LogError("CircleAnim: Drag the Player into the Reference slot!");
    }

    void Update()
    {
        if (playerScript == null) return;

        // Check if moveInput (now public) is being used
        bool isMoving = playerScript.moveInput.sqrMagnitude > 0.001f;

        if (isMoving)
        {
            // The Flip-Flop
            float wave = Mathf.Sin(Time.time * pulseSpeed) * pulseMagnitude;
            transform.localScale = new Vector3(
                originalScale.x + wave,
                originalScale.y - wave,
                originalScale.z
            );
        }
        else
        {
            // 1. Smoothly slide back to original scale
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * returnSpeed);

            // 2. Hard Reset (The "Snap") 
            // If we are very close to the original, just set it exactly
            if (Vector3.Distance(transform.localScale, originalScale) < 0.001f)
            {
                transform.localScale = originalScale;
            }
        }
    }
}