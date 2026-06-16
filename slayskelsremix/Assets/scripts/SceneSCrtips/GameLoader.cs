using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public void StartNewGame()
    {
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
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