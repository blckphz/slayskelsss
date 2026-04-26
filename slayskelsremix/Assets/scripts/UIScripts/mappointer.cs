using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapPointer : MonoBehaviour
{
    private Transform target;
    private Vector3 offset = new Vector3(0, 1.25f, 0);
    private float margin = 50f;

    [SerializeField] private Image pointerImage;
    [SerializeField] private TextMeshProUGUI distanceText;

    private Camera mainCam;
    private CanvasGroup canvasGroup; // Smoother fading than color swapping

    public void Initialize(Transform targetTransform, Camera cam, Sprite icon)
    {
        target = targetTransform;
        mainCam = cam;
        canvasGroup = GetComponent<CanvasGroup>();

        if (pointerImage == null) pointerImage = GetComponentInChildren<Image>();
        if (distanceText == null) distanceText = GetComponentInChildren<TextMeshProUGUI>();

        if (icon != null && pointerImage != null)
            pointerImage.sprite = icon;
    }

    public Transform GetTarget() => target;

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Calculate basic screen position
        Vector3 targetWorldPos = target.position + offset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(targetWorldPos);

        // 2. Handle objects behind the camera
        bool isBehind = screenPos.z < 0;
        if (isBehind)
        {
            screenPos *= -1;
        }

        // 3. Update Distance Text (Cached check)
        if (distanceText != null)
        {
            float dist = Vector3.Distance(mainCam.transform.position, target.position);
            distanceText.text = $"{Mathf.RoundToInt(dist)}m";
        }

        // 4. Edge Clamping Logic (Vector-based instead of Trig-based)
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) * 0.5f;
        screenPos -= screenCenter;

        float angle = Mathf.Atan2(screenPos.y, screenPos.x);
        float slope = screenPos.y / screenPos.x;

        Vector3 bounds = screenCenter - new Vector3(margin, margin, 0);

        if (isBehind || Mathf.Abs(screenPos.x) > bounds.x || Mathf.Abs(screenPos.y) > bounds.y)
        {
            // Clamp to screen edges
            if (Mathf.Abs(screenPos.x * bounds.y) > Mathf.Abs(screenPos.y * bounds.x))
            {
                // Hit left/right
                float xSign = screenPos.x > 0 ? 1 : -1;
                screenPos = new Vector3(xSign * bounds.x, xSign * bounds.x * slope, 0);
            }
            else
            {
                // Hit top/bottom
                float ySign = screenPos.y > 0 ? 1 : -1;
                screenPos = new Vector3(ySign * bounds.y / slope, ySign * bounds.y, 0);
            }

            if (canvasGroup != null) canvasGroup.alpha = 0.6f;
        }
        else
        {
            if (canvasGroup != null) canvasGroup.alpha = 1.0f;
        }

        transform.position = screenPos + screenCenter;
    }
}