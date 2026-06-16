using UnityEngine;

public class CircleAnim : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerScript;
    public TiltManager tiltManager;

    public AudioSource stepAudioSource;

    public AudioClip defaultStepClip;
    public AudioClip soilStepClip;

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

        Debug.Log("[CircleAnim] Initialized");

        if (playerScript == null)
            Debug.LogError("[CircleAnim] Missing PlayerMovement");

        if (stepAudioSource == null)
            Debug.LogError("[CircleAnim] Missing AudioSource");

        if (tiltManager == null)
            Debug.LogWarning("[CircleAnim] Missing TiltManager (soil detection disabled)");
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

        bool crossed = (wave < 0f && lastWaveValue >= 0f) ||
                       (wave > 0f && lastWaveValue <= 0f);

        if (!stepTriggered && crossed)
        {
            stepTriggered = true;

            Debug.Log("[CircleAnim] STEP TRIGGERED");

            PlayStepSound();
            SpawnParticles();
        }

        if (Mathf.Abs(wave) > 0.1f)
            stepTriggered = false;

        lastWaveValue = wave;
    }

    void PlayStepSound()
    {
        if (stepAudioSource == null) return;

        AudioClip clipToPlay = defaultStepClip;

        if (tiltManager != null && tiltManager.Tilemap != null && playerScript != null)
        {
            Vector3 pos = playerScript.transform.position;
            pos.z = 0f;

            Vector3Int cell = tiltManager.Tilemap.WorldToCell(pos);
            cell.z = 0;

            bool hasTile = tiltManager.Tilemap.GetTile(cell) != null;

            Debug.Log($"[CircleAnim] Checking cell {cell} → Tile: {(hasTile ? "YES" : "NO")}");

            if (hasTile)
                clipToPlay = soilStepClip;
        }
        else
        {
            Debug.Log("[CircleAnim] Using default sound (missing references)");
        }

        if (clipToPlay == null)
        {
            Debug.LogError("[CircleAnim] clipToPlay is NULL");
            return;
        }

        stepAudioSource.pitch = Random.Range(0.9f, 1.1f);
        stepAudioSource.PlayOneShot(clipToPlay);

        Debug.Log($"[CircleAnim] Played: {clipToPlay.name}");
    }

    void SpawnParticles()
    {
        if (stepParticlePrefab == null) return;

        Transform spawn = particleSpawnPoint != null ? particleSpawnPoint : transform;

        Vector3 offset = new Vector3(
            Random.Range(-0.05f, 0.05f),
            0f,
            Random.Range(-0.05f, 0.05f)
        );

        Instantiate(stepParticlePrefab, spawn.position + offset, Quaternion.identity);

        Debug.Log("[CircleAnim] Particle spawned");
    }
}