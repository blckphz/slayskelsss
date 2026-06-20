using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider volumeSlider;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction; // Bind this to Esc

    [Header("UI")]
    [SerializeField] private GameObject optionsPanel;

    private bool isOpen;

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += ToggleOptions;
    }

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found!");
            return;
        }

        volumeSlider.value = AudioManager.Instance.GetMasterVolume();
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        optionsPanel.SetActive(false);
    }

    private void ToggleOptions(InputAction.CallbackContext context)
    {
        isOpen = !isOpen;
        optionsPanel.SetActive(isOpen);

        // Optional: pause game
        Time.timeScale = isOpen ? 0f : 1f;

        // Optional: unlock cursor
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }

    private void OnVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= ToggleOptions;
        pauseAction.action.Disable();
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}