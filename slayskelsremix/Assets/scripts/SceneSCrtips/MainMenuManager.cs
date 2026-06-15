using TMPro;
using UnityEngine;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text playButtonText;
    [SerializeField] private GameLoader gameLoader;

    private void Start()
    {
        Debug.Log("MainMenuManager Start running");

        string savePath =
            Application.persistentDataPath + "/buildings.json";

        Debug.Log("Save exists: " + File.Exists(savePath));

        if (playButtonText == null)
        {
            Debug.LogError("playButtonText is NOT assigned!");
            return;
        }

        playButtonText.text =
            File.Exists(savePath)
            ? "Continue"
            : "New Game";
    }
    public void PlayGame()
    {
        GameLoadingState.WorldReady = false;

        // IMPORTANT: use loader instead of direct scene load
        gameLoader.StartNewGame();
    }
}