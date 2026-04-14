using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    public GameObject pressEPrompt;
    public TextMeshProUGUI promptText;

    private void Awake()
    {
        Instance = this;
        pressEPrompt.SetActive(false);
    }

    public void Show(string text)
    {
        promptText.text = text;
        pressEPrompt.SetActive(true);
    }

    public void Hide()
    {
        pressEPrompt.SetActive(false);
    }
}