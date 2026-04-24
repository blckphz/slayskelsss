using UnityEngine;

public class CircleAnim : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerScript;

    public AudioSource stepAudioSource;
    public AudioClip stepClip;

    public GameObject stepParticlePrefab;
    public Transform particleSpawnPoint;

    [Header("Animation Settings")]
    public float pulseSpeed = 12f;
    public float pulseMagnitude = 0.15f;
    public float returnSpeed = 10f;

    private Vector3 originalScale;

    // Step detection
    private float lastWaveValue;
    private bool stepTriggered;

    void Start()
    {
        originalScale = transform.localScale;

        if (playerScript == null)
            Debug.LogError("CircleAnim: Drag the Player into the Reference slot!");
    }

    void Update()
    {
        if (playerScript == null) return;

        bool isMoving = playerScript.moveInput.sqrMagnitude > 0.001f;

        HandleAnimation(isMoving);
        HandleStepsFromWave(isMoving);
    }

    void HandleAnimation(bool isMoving)
    {
        if (isMoving)
        {
            float wave = Mathf.Sin(Time.time * pulseSpeed) * pulseMagnitude;

            transform.localScale = new Vector3(
                originalScale.x + wave,
                originalScale.y - wave,
                originalScale.z
            );
        }
        else
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                Time.deltaTime * returnSpeed
            );

            if (Vector3.Distance(transform.localScale, originalScale) < 0.001f)
            {
                transform.localScale = originalScale;
            }
        }
    }

    void HandleStepsFromWave(bool isMoving)
    {
        if (!isMoving)
        {
            stepTriggered = false;
            lastWaveValue = 0f;
            return;
        }

        float wave = Mathf.Sin(Time.time * pulseSpeed);

        bool crossedDown = (wave < 0f && lastWaveValue >= 0f);
        bool crossedUp = (wave > 0f && lastWaveValue <= 0f);

        if (!stepTriggered && (crossedDown || crossedUp))
        {
            stepTriggered = true;

            // 🎲 RANDOM PITCH (main juice)
            if (stepAudioSource != null && stepClip != null)
            {
                stepAudioSource.pitch = Random.Range(0.9f, 1.1f);
                stepAudioSource.PlayOneShot(stepClip);
            }

            // 💨 PARTICLE with slight offset
            if (stepParticlePrefab != null)
            {
                Transform spawn = particleSpawnPoint != null ? particleSpawnPoint : transform;

                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.05f, 0.05f),
                    0f,
                    Random.Range(-0.05f, 0.05f)
                );

                Instantiate(stepParticlePrefab, spawn.position + randomOffset, Quaternion.identity);
            }
        }

        // reset trigger zone
        if (wave > 0.1f || wave < -0.1f)
        {
            stepTriggered = false;
        }

        lastWaveValue = wave;
    }
}