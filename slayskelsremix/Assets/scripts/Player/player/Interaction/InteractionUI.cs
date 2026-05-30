using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [Header("References")]
    public GameObject pressEPrompt;
    public TextMeshProUGUI promptText;

    private Coroutine messageRoutine;

    private void Awake()
    {
        Instance = this;

        Debug.Log("InteractionUI initialized");

        if (pressEPrompt != null)
            pressEPrompt.SetActive(false);
    }

    // =========================================================
    // NORMAL SHOW
    // =========================================================
    public void Show(string text)
    {
        Debug.Log("InteractionUI.Show(): " + text);

        if (promptText != null)
            promptText.text = text;

        if (pressEPrompt != null)
            pressEPrompt.SetActive(true);
    }

    // =========================================================
    // HIDE
    // =========================================================
    public void Hide()
    {
        Debug.Log("InteractionUI.Hide()");

        if (pressEPrompt != null)
            pressEPrompt.SetActive(false);
    }

    // =========================================================
    // TEMPORARY MESSAGE
    // =========================================================
    public void ShowTemporary(string text, float duration = 1f)
    {
        Debug.Log("ShowTemporary(): " + text);

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(
            TemporaryMessage(text, duration)
        );
    }

    private IEnumerator TemporaryMessage(
        string text,
        float duration
    )
    {
        Show(text);

        yield return new WaitForSeconds(duration);

        Hide();

        messageRoutine = null;
    }
}