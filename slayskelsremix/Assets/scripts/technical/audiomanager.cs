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
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved volume
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        // Audio source
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Recommended defaults
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D audio
    }

    // =====================================================
    // VOLUME CONTROL
    // =====================================================

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }

    public float GetMasterVolume()
    {
        return masterVolume;
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
    // UI SOUND
    // =====================================================

    public void PlayUISound(AudioClip clip, float volume = 1f, bool useRandomPitch = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Tried to play NULL UI clip.");
            return;
        }

        if (useRandomPitch && randomizePitch)
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
}