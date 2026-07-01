using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource rainSource;
    [SerializeField] private AudioSource ambienceSource;

    [Header("Main Menu Clips")]
    public AudioClip menuMusic;
    public AudioClip menuRain;
    public AudioClip menuAmbience;

    [Header("Game Clips")]
    public AudioClip gameMusic;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        AudioManager.OnMusicVolumeChanged += SyncMusicVolume;
        AudioManager.OnSFXVolumeChanged += SyncSFXVolume;

        if (AudioManager.Instance != null)
        {
            SyncMusicVolume(AudioManager.Instance.GetMusicVolume());
            SyncSFXVolume(AudioManager.Instance.GetSFXVolume());
        }

        UpdateAudio(SceneManager.GetActiveScene().name);
    }

    private void SyncMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    private void SyncSFXVolume(float volume)
    {
        rainSource.volume = volume;
        ambienceSource.volume = volume;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateAudio(scene.name);
    }

    private void UpdateAudio(string sceneName)
    {
        if (sceneName == "MainMenuScene")
        {
            PlayLoop(musicSource, menuMusic);
            PlayLoop(rainSource, menuRain);
            PlayLoop(ambienceSource, menuAmbience);
        }
        else if (sceneName == "GameScene")
        {
            PlayLoop(musicSource, gameMusic);

            rainSource.Stop();
            ambienceSource.Stop();
        }
    }

    private void PlayLoop(AudioSource source, AudioClip clip)
    {
        if (clip == null) return;

        if (source.clip != clip)
        {
            source.clip = clip;
            source.loop = true;
            source.Play();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        AudioManager.OnMusicVolumeChanged -= SyncMusicVolume;
        AudioManager.OnSFXVolumeChanged -= SyncSFXVolume;
    }
}