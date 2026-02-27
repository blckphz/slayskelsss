using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingScreen;
    public Slider progressBar;

    public void LoadLevel(int sceneIndex)
    {
        // Start a Coroutine so the game keeps running while loading
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        // 1. Show the loading UI
        loadingScreen.SetActive(true);

        // 2. Start the background operation
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        // 3. Loop until the scene is finished loading
        while (!operation.isDone)
        {
            // Unity loads scenes in two steps. 0.0 to 0.9 is loading. 
            // 0.9 to 1.0 is activation. We clamp it for a smooth 0-1 value.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // 4. Update the slider value
            progressBar.value = progress;

            // Wait until the next frame to update again
            yield return null;
        }
    }
}