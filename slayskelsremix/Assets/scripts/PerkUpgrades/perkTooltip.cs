using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class perkTooltip : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI tooltipName;
    public TextMeshProUGUI tooltipInfo;

    [Header("Settings")]
    public Vector2 offset = new Vector2(20, -20);

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_rectTransform == null)
        {
            Debug.LogError("[perkTooltip] Missing RectTransform!");
            return;
        }

        _rectTransform.pivot = new Vector2(0, 1);

        gameObject.SetActive(false);

        Debug.Log("[perkTooltip] Initialized.");
    }

    private void Update()
    {
        if (gameObject.activeSelf)
            FollowMouse();
    }

    private void FollowMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        _rectTransform.position = mousePosition + offset;
    }

    public void ShowTooltip(string perkName, string description, int level, int maxLevel)
    {
        if (tooltipName == null || tooltipInfo == null)
        {
            Debug.LogError("[perkTooltip] Missing TMP references!");
            return;
        }

        Debug.Log($"[perkTooltip] Showing tooltip: {perkName} ({level}/{maxLevel})");

        gameObject.SetActive(true);

        tooltipName.text = perkName;

        tooltipInfo.text =
            $"<b>Level {level}/{maxLevel}</b>\n\n" +
            description;

        FollowMouse();
    }

    public void HideTooltip()
    {
        Debug.Log("[perkTooltip] Hiding tooltip.");

        gameObject.SetActive(false);
    }
}