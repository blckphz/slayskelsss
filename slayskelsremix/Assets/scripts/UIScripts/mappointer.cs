using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapPointer : MonoBehaviour
{
    private Transform target;
    private Vector3 offset = new Vector3(0, 1.25f, 0);
    private float margin = 50f;

    [Header("Visual References")]
    [SerializeField] private Image pointerImage; // Drag the child icon here
    [SerializeField] private TextMeshProUGUI distanceText; // Drag the child text here

    private Camera mainCam;

    public void Initialize(Transform targetTransform, Camera cam, Sprite icon)
    {
        target = targetTransform;
        mainCam = cam;

        // Fallback: If not assigned in inspector, try to find in children
        if (pointerImage == null) pointerImage = GetComponentInChildren<Image>();
        if (distanceText == null) distanceText = GetComponentInChildren<TextMeshProUGUI>();

        if (icon != null && pointerImage != null)
            pointerImage.sprite = icon;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Get positions
        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position + offset);
        bool isBehind = screenPos.z < 0;

        if (isBehind) screenPos *= -1;

        // 2. Handle distance display
        if (distanceText != null)
        {
            float dist = Vector3.Distance(mainCam.transform.position, target.position);
            distanceText.text = Mathf.Round(dist) + "m";
        }

        // 3. Screen Clamping Logic
        float minX = margin;
        float maxX = Screen.width - margin;
        float minY = margin;
        float maxY = Screen.height - margin;

        if (isBehind || screenPos.x < minX || screenPos.x > maxX || screenPos.y < minY || screenPos.y > maxY)
        {
            Vector3 cappedScreenPos = screenPos;
            cappedScreenPos.x -= Screen.width / 2;
            cappedScreenPos.y -= Screen.height / 2;

            float angle = Mathf.Atan2(cappedScreenPos.y, cappedScreenPos.x);
            float m = Mathf.Sin(angle) / Mathf.Cos(angle);
            float screenAspect = (float)Screen.height / Screen.width;

            if (Mathf.Abs(m) <= screenAspect)
            {
                float x = Mathf.Cos(angle) > 0 ? Screen.width / 2 - margin : -Screen.width / 2 + margin;
                float y = m * x;
                screenPos = new Vector3(x + Screen.width / 2, y + Screen.height / 2, 0);
            }
            else
            {
                float y = Mathf.Sin(angle) > 0 ? Screen.height / 2 - margin : -Screen.height / 2 + margin;
                float x = y / m;
                screenPos = new Vector3(x + Screen.width / 2, y + Screen.height / 2, 0);
            }

            // Apply fade to the ICON specifically
            if (pointerImage != null) pointerImage.color = new Color(1, 1, 1, 0.6f);
        }
        else
        {
            // Fully visible
            if (pointerImage != null) pointerImage.color = new Color(1, 1, 1, 1f);
        }

        transform.position = screenPos;
    }
}