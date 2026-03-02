using UnityEngine;
using TMPro;

public class comboScript : MonoBehaviour
{
    [Header("Multi-Kill Settings")]
    public float multiKillWindow = 2.0f; // Seconds to get next kill
    public float scaleFactor = 1.4f;
    public float lerpSpeed = 8f;

    [Header("Scoring")]
    public TextMeshProUGUI scoreText;
    private int totalScore = 0;

    [Header("UI Notifications")]
    public TextMeshProUGUI comboText; // Shows "x3"
    public TextMeshProUGUI rankText;  // Shows "TRIPLE KILL!"

    private int currentStreak = 0;
    private float lastKillTime;
    private Vector3 originalScale;

    void Start()
    {
        if (comboText != null)
        {
            originalScale = comboText.transform.localScale;
            comboText.gameObject.SetActive(false);
        }
        if (rankText != null) rankText.gameObject.SetActive(false);
        UpdateScoreUI();
    }

    void Update()
    {
        // Reset streak if too much time passes
        if (currentStreak > 0 && Time.time - lastKillTime > multiKillWindow)
        {
            ResetCombo();
        }

        // Juice: Scale the text back down smoothly
        if (comboText != null && comboText.gameObject.activeSelf)
        {
            comboText.transform.localScale = Vector3.Lerp(comboText.transform.localScale, originalScale, Time.deltaTime * lerpSpeed);
        }
    }

    public void RegisterKill()
    {
        currentStreak++;
        lastKillTime = Time.time;

        // Logic: 1st kill = 1pt, 2nd kill = 2pts, 3rd kill = 3pts
        totalScore += currentStreak;

        UpdateScoreUI();
        UpdateComboUI();

        // Pop the text size
        if (comboText != null) comboText.transform.localScale = originalScale * scaleFactor;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "" + totalScore;
    }

    private void UpdateComboUI()
    {
        if (comboText == null) return;

        comboText.gameObject.SetActive(true);
        comboText.text = "x" + currentStreak;

        if (rankText != null)
        {
            rankText.gameObject.SetActive(true);
            rankText.text = GetRankName(currentStreak);
        }
    }

    private string GetRankName(int streak)
    {
        return streak switch
        {
            1 => "KILL!",
            2 => "DOUBLE KILL!",
            3 => "TRIPLE KILL!",
            4 => "QUADRA KILL!",
            5 => "PENTA KILL!",
            _ => "LEGENDARY!!"
        };
    }

    public void ResetCombo()
    {
        currentStreak = 0;
        if (comboText != null) comboText.gameObject.SetActive(false);
        if (rankText != null) rankText.gameObject.SetActive(false);
    }
}