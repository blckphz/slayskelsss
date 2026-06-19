using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{

    public GameObject loadingScreenPrefab;
    public GameObject blackbg;

    public void StartNewGame()
    {
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        loadingScreenPrefab.SetActive(true);
        blackbg.SetActive(true);

        LoadingScreen.Instance?.SetText("Loading Game Scene...");

        AsyncOperation op = SceneManager.LoadSceneAsync("GameScene");

        while (!op.isDone)
        {
            yield return null;
        }

        LoadingScreen.Instance?.SetText("Preparing World...");

        while (!GameLoadingState.WorldReady)
        {
            yield return null;
        }

        LoadingScreen.Instance?.SetText("Done!");

        yield return new WaitForSeconds(0.5f);

        LoadingScreen.Instance?.FinishLoading();
    }
}