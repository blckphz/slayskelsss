using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Events
    public static event Action<float> OnSFXVolumeChanged;
    public static event Action<float> OnMusicVolumeChanged;

    private AudioSource audioSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;

    [Header("Pitch Randomization")]
    public bool randomizePitch = true;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    //========================
    // SFX
    //========================

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();

        OnSFXVolumeChanged?.Invoke(sfxVolume);
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    //========================
    // Music
    //========================

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();

        OnMusicVolumeChanged?.Invoke(musicVolume);
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    //========================
    // Sound Effects
    //========================

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        audioSource.pitch = randomizePitch
            ? UnityEngine.Random.Range(minPitch, maxPitch)
            : 1f;

        audioSource.PlayOneShot(clip, volume * sfxVolume);
    }

    public void PlayUISound(AudioClip clip, float volume = 1f, bool useRandomPitch = false)
    {
        if (clip == null) return;

        audioSource.pitch = (useRandomPitch && randomizePitch)
            ? UnityEngine.Random.Range(minPitch, maxPitch)
            : 1f;

        audioSource.PlayOneShot(clip, volume * sfxVolume);
    }
}