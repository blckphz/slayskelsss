using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScript : MonoBehaviour
{

    public GameObject GameOverUIPanel;

    public void Start()
    {
        GameOverUIPanel.SetActive(false);
    }



    public void LoadLastSave()
    {
        GameOverUIPanel.SetActive(false);


        SceneManager.LoadScene("GameScene");
    }

    public void GoToMainMenu()
    {
        GameOverUIPanel.SetActive(false);

        SceneManager.LoadScene("MainMenuScene");
    }
}