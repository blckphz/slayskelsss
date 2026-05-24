using UnityEngine;

public class HotbarBuildModeUI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveUpAmount = 10f;
    [SerializeField] private float moveDownAmount = 40f;
    [SerializeField] private float animationSpeed = 8f;
    [SerializeField] private float bounceDuration = 0.08f;

    [Header("Fade")]
    [SerializeField] private float normalAlpha = 1f;
    [SerializeField] private float buildModeAlpha = 0.35f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private Vector2 originalPos;
    private Vector2 targetPos;
    private float targetAlpha;

    private bool doingBounce;
    private float bounceTimer;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalPos = rect.anchoredPosition;

        targetPos = originalPos;
        targetAlpha = normalAlpha;
    }

    private void Update()
    {
        if (doingBounce)
        {
            bounceTimer -= Time.unscaledDeltaTime;

            if (bounceTimer <= 0f)
            {
                doingBounce = false;

                // after upward pop, go down into build mode position
                targetPos = originalPos + Vector2.down * moveDownAmount;
                targetAlpha = buildModeAlpha;
            }
        }

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
        if (active)
        {
            // pop UP first
            targetPos = originalPos + Vector2.up * moveUpAmount;
            targetAlpha = normalAlpha;

            doingBounce = true;
            bounceTimer = bounceDuration;
        }
        else
        {
            // return to normal
            doingBounce = false;
            targetPos = originalPos;
            targetAlpha = normalAlpha;
        }
    }
}