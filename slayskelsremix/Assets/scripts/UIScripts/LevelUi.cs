using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUi : MonoBehaviour
{
    [Header("References")]
    public LevelManager playerLevelManager;

    [Header("UI Elements")]
    public Slider xpSlider;
    public TMP_Text statusText; // Combined text for Level and Points
    public TMP_Text xpNumbersText; // Optional: "50 / 150 XP"

    void Start()
    {
        if (playerLevelManager == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerLevelManager = player.GetComponent<LevelManager>();
        }
    }

    void Update()
    {
        if (playerLevelManager != null) UpdateUI();
    }

    void UpdateUI()
    {
        // Combined line: LV: X / Skillpoints: Y
        if (statusText != null)
        {
            statusText.text = $"LV: {playerLevelManager.level} / Skillpoints: {playerLevelManager.skillPoints}";
        }

        // Update the Slider
        if (xpSlider != null)
        {
            xpSlider.maxValue = playerLevelManager.xpToNextLevel;
            xpSlider.value = playerLevelManager.currentXP;
        }

        // Optional XP fraction text
        if (xpNumbersText != null)
            xpNumbersText.text = $"{playerLevelManager.currentXP} / {playerLevelManager.xpToNextLevel} XP";
    }
}