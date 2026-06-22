using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [Header("Main Menu")]
    public AudioClip menuMusic;
    public AudioClip menuRain;
    public AudioClip menuAmbience;

    [Header("Game")]
    public AudioClip gameMusic;

    private AudioSource musicSource;
    private AudioSource rainSource;
    private AudioSource ambienceSource;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        AudioSource[] sources = GetComponents<AudioSource>();

        musicSource = sources[0];
        rainSource = sources[1];
        ambienceSource = sources[2];

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        UpdateAudio(SceneManager.GetActiveScene().name);
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
        if (source.clip != clip)
        {
            source.clip = clip;
            source.loop = true;
            source.Play();
        }
    }
}