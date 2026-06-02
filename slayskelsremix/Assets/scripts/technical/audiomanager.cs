using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Header("Pitch Randomization")]
    public bool randomizePitch = true;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // audio source
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // recommended defaults
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D audio
    }

    // =====================================================
    // MAIN SOUND METHOD
    // =====================================================

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Tried to play NULL clip.");
            return;
        }

        // random pitch variation
        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        audioSource.PlayOneShot(
            clip,
            volume * masterVolume
        );
    }

    // =====================================================
    // UI SOUND (NO RANDOM PITCH)
    // =====================================================

    public void PlayUISound(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Tried to play NULL UI clip.");
            return;
        }

        audioSource.pitch = 1f;

        audioSource.PlayOneShot(
            clip,
            volume * masterVolume
        );
    }
}