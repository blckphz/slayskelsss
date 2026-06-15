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
        // ensure loading UI exists
        SceneManager.LoadScene("LoadingScene");

        // wait 1 frame so LoadingScreen spawns
        yield return null;

        LoadingScreen.Instance?.SetText("Loading Game Scene...");

        AsyncOperation op =
            SceneManager.LoadSceneAsync("GameScene");

        op.allowSceneActivation = true;

        // wait until scene fully loaded
        while (!op.isDone)
        {
            yield return null;
        }

        LoadingScreen.Instance?.SetText("Preparing World...");

        // IMPORTANT: wait for ResourceSpawner to finish
        while (GameLoadingState.WorldReady == false)
        {
            yield return null;
        }

        LoadingScreen.Instance?.SetText("Done!");

        yield return new WaitForSeconds(0.5f);

        LoadingScreen.Instance?.FinishLoading();
    }
}