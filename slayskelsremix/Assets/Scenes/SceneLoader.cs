using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("UI Elements")]
    public GameObject loadingScreen;
    public Slider progressBar;
    public TextMeshProUGUI loadingText;

    [Header("Settings")]
    public float minimumLoadTime = 2f;

    private bool customTextActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    public void LoadLevel(int sceneIndex)
    {
        customTextActive = false;
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    public void SetLoadingText(string message)
    {
        // lock custom messages so SceneLoader won't overwrite them
        customTextActive = true;

        Debug.Log($"[Loading] {message}");

        if (loadingText != null)
            loadingText.text = message;
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        customTextActive = false;

        loadingText.text = "Loading scene...";

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (operation.progress < 0.9f || timer < minimumLoadTime)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            // Only use default messages if no custom save/load text is active
            if (!customTextActive)
            {
                if (progress < 0.3f)
                    loadingText.text = "Preparing world...";
                else if (progress < 0.6f)
                    loadingText.text = "Loading assets...";
                else if (progress < 0.9f)
                    loadingText.text = "Generating world...";
                else
                    loadingText.text = "Finalizing...";
            }

            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        if (!customTextActive)
            loadingText.text = "Entering world...";

        yield return new WaitForSeconds(0.5f);

        operation.allowSceneActivation = true;

        yield return null;

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}