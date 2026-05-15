using UnityEngine;
using System.Collections;

public class UIShaker : MonoBehaviour
{
    public static UIShaker Instance { get; private set; }

    private RectTransform rectTransform;
    private Vector2 initialAnchoredPos;
    private Coroutine shakeCoroutine;

    [Tooltip("Global multiplier for the shake offset")]
    public float intensity = 1.0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        rectTransform = GetComponent<RectTransform>();
        initialAnchoredPos = rectTransform.anchoredPosition;
    }

    public void ShakeUI(float duration, float magnitude)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Using intensity * magnitude to determine the max pixel offset
            float x = Random.Range(-intensity, intensity) * magnitude;
            float y = Random.Range(-intensity, intensity) * magnitude;

            rectTransform.anchoredPosition = initialAnchoredPos + new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = initialAnchoredPos;
    }
}