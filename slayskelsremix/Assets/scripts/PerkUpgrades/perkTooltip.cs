using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // Required for Mouse.current

public class perkTooltip : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI tooltipName;
    public TextMeshProUGUI tooltipInfo;

    [Header("Settings")]
    public Vector2 offset = new Vector2(20, -20); // Down and to the right

    private RectTransform _rectTransform;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        // Ensure the Pivot is set to Top-Left (0, 1) in the Inspector 
        // so it doesn't flicker under the mouse cursor.
        _rectTransform.pivot = new Vector2(0, 1);

        gameObject.SetActive(false);
    }

    void Update()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        if (Mouse.current == null) return;

        // Get mouse position from the New Input System
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Apply position with offset
        _rectTransform.position = mousePosition + offset;

        // Optional: Keep tooltip on screen
        ClampToWindow();
    }

    private void ClampToWindow()
    {
        Vector3[] corners = new Vector3[4];
        _rectTransform.GetWorldCorners(corners);

        float width = corners[2].x - corners[0].x;
        float height = corners[1].y - corners[0].y;

        Vector2 pos = _rectTransform.position;

        // Check right edge
        if (pos.x + width > Screen.width)
            pos.x = Screen.width - width;

        // Check bottom edge
        if (pos.y - height < 0)
            pos.y = height;

        _rectTransform.position = pos;
    }

    public void ShowTooltip(string name, string description, int lv, int maxLv)
    {
        gameObject.SetActive(true);
        tooltipName.text = name;
        tooltipInfo.text = $"<b>Lv {lv}/{maxLv}</b>\n{description}";

        // Force update position immediately so it doesn't "jump" from last known pos
        FollowMouse();
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}