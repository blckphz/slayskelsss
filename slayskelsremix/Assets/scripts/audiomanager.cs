using UnityEngine;

public class audiomanager : MonoBehaviour
{
    public static audiomanager Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            // Set a random pitch between 0.8 and 1.2
            audioSource.pitch = Random.Range(0.9f, 1.1f);

            audioSource.PlayOneShot(clip, volume);
        }
    }
}