using TMPro;
using UnityEngine;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text playButtonText;
    [SerializeField] private GameLoader gameLoader;
    [SerializeField] private GameObject newGamePanel;

    private string savePath;

    private void Start()
    {
        Debug.Log("MainMenuManager Start running");

        savePath = Path.Combine(Application.persistentDataPath, "buildings.json");

        bool saveExists = File.Exists(savePath);

        Debug.Log("Save exists: " + saveExists);

        if (playButtonText == null)
        {
            Debug.LogError("playButtonText is NOT assigned!");
            return;
        }

        playButtonText.text = saveExists ? "Continue" : "New Game";

        if (newGamePanel != null)
            newGamePanel.SetActive(false);
    }

    public void PlayGame()
    {
        GameLoadingState.WorldReady = false;

        if (File.Exists(savePath))
        {
            // Continue existing game
            gameLoader.StartNewGame();
        }
        else
        {
            // Show New Game setup panel
            if (newGamePanel != null)
                newGamePanel.SetActive(true);
        }
    }

    public void ConfirmNewGame()
    {
        gameLoader.StartNewGame();
    }

    public void CancelNewGame()
    {
        if (newGamePanel != null)
            newGamePanel.SetActive(false);
    }
}