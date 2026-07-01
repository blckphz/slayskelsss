using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("UI")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject backToMenuButton;

    private bool isOpen;

    private void OnEnable()
    {
        Debug.Log("OptionsMenu enabled.");

        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += ToggleOptions;
        }
        else
        {
            Debug.LogError("Pause Action is NOT assigned!");
        }
    }

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found!");
            return;
        }

        sfxSlider.value = AudioManager.Instance.GetSFXVolume();
        musicSlider.value = AudioManager.Instance.GetMusicVolume();

        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);

        optionsPanel.SetActive(false);

        Debug.Log("Current Scene: " + SceneManager.GetActiveScene().name);

        if (backToMenuButton == null)
        {
            Debug.LogError("Back To Menu Button is NOT assigned!");
        }
        else
        {
            Debug.Log("Back To Menu Button assigned to: " + backToMenuButton.name);
            Debug.Log("Button active at Start: " + backToMenuButton.activeSelf);
        }
    }

    public void ToggleOptions(InputAction.CallbackContext context)
    {
        Debug.Log($"Pause action triggered. Phase: {context.phase}");

        if (context.performed)
        {
            Debug.Log("ESC key pressed.");
            ToggleOptionsFunction();
        }
    }

    public void ToggleOptionsFunction()
    {
        isOpen = !isOpen;

        Debug.Log("--------------------------------");
        Debug.Log("Current Scene: " + SceneManager.GetActiveScene().name);
        Debug.Log("Options Open: " + isOpen);

        optionsPanel.SetActive(isOpen);

        if (backToMenuButton == null)
        {
            Debug.LogError("backToMenuButton is NULL!");
            return;
        }

        Debug.Log("Button before change: " + backToMenuButton.activeSelf);

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            Debug.Log("Detected GameScene.");

            backToMenuButton.SetActive(isOpen);

            Debug.Log("Button after SetActive(" + isOpen + "): " + backToMenuButton.activeSelf);
        }
        else
        {
            Debug.Log("NOT GameScene.");

            backToMenuButton.SetActive(false);

            Debug.Log("Button after SetActive(false): " + backToMenuButton.activeSelf);
        }

        Time.timeScale = isOpen ? 0f : 1f;
    }

    public void BackToMainMenu()
    {
        Debug.Log("Back To Menu button clicked.");

        Time.timeScale = 1f;
        isOpen = false;

        optionsPanel.SetActive(false);

        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnSFXChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }

    private void OnMusicChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnDisable()
    {
        Debug.Log("OptionsMenu disabled.");

        if (pauseAction != null)
        {
            pauseAction.action.performed -= ToggleOptions;
            pauseAction.action.Disable();
        }
    }
}