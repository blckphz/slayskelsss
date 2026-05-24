using UnityEngine;

public class HotbarBuildModeUI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveDownAmount = 25f;
    [SerializeField] private float animationSpeed = 8f;

    [Header("Fade")]
    [SerializeField] private float normalAlpha = 1f;
    [SerializeField] private float buildModeAlpha = 0.35f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private Vector2 originalPos;
    private Vector2 targetPos;
    private float targetAlpha;

    private bool buildMode;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        originalPos = rect.anchoredPosition;
        targetPos = originalPos;
        targetAlpha = normalAlpha;

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Update()
    {
        rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            targetPos,
            Time.unscaledDeltaTime * animationSpeed
        );

        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            targetAlpha,
            Time.unscaledDeltaTime * animationSpeed
        );
    }

    public void SetBuildMode(bool active)
    {
        buildMode = active;

        if (buildMode)
        {
            targetPos = originalPos + Vector2.down * moveDownAmount;
            targetAlpha = buildModeAlpha;
        }
        else
        {
            targetPos = originalPos;
            targetAlpha = normalAlpha;
        }
    }
}