using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [Header("References")]
    public GameObject pressEPrompt;
    public TextMeshProUGUI promptText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float fadeSpeed = 8f;
    public float defaultDuration = 1.5f;

    private Coroutine fadeRoutine;
    private Coroutine hideRoutine;

    private void Awake()
    {
        Instance = this;

        if (pressEPrompt != null)
            pressEPrompt.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    // =========================================================
    // SHOW (ALWAYS USE DEFAULT DURATION)
    // =========================================================
    public void Show(string text)
    {
        if (promptText != null)
            promptText.text = text;

        if (pressEPrompt != null)
            pressEPrompt.SetActive(true);

        StartFade(1f);

        RestartAutoHide();
    }

    // =========================================================
    // HIDE
    // =========================================================
    public void Hide()
    {
        StartFade(0f);
    }

    // =========================================================
    // ALWAYS USE DEFAULT TIMER
    // =========================================================
    private void RestartAutoHide()
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(AutoHide());
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(defaultDuration);
        Hide();
    }

    // =========================================================
    // FADE SYSTEM
    // =========================================================
    private void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (canvasGroup == null)
            yield break;

        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f && pressEPrompt != null)
            pressEPrompt.SetActive(false);
    }
}