using UnityEngine;
using TMPro;

public class comboScript : MonoBehaviour
{
    [Header("Combo Settings (Steady Flow)")]
    public float multiKillWindow = 3.0f; // Time allowed between any kill to keep combo alive
    public float scaleFactor = 1.4f;
    public float lerpSpeed = 8f;

    [Header("Multi-Kill Settings (Simultaneous)")]
    [Tooltip("The tiny window (e.g. 0.2s) to group kills into one 'Multi-Kill' event")]
    public float simultaneousGracePeriod = 0.2f;

    [Header("Scoring")]
    public TextMeshProUGUI scoreText;
    private int totalScore = 0;

    [Header("UI Notifications")]
    public TextMeshProUGUI comboText; // Shows "x15"
    public TextMeshProUGUI rankText;  // Shows "TRIPLE KILL!"

    private int currentStreak = 0;   // This is your persistent Combo
    private int burstKillCount = 0;  // This counts kills happening "at once"
    private float lastKillTime;
    private float burstTimer;
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
        // 1. Reset Combo if too much time passes between kills
        if (currentStreak > 0 && Time.time - lastKillTime > multiKillWindow)
        {
            ResetCombo();
        }

        // 2. Check if the "Simultaneous" window has closed to announce Multi-Kill
        if (burstKillCount > 0 && Time.time - burstTimer > simultaneousGracePeriod)
        {
            HandleMultiKillAnnouncement();
        }

        // Juice: Scale the combo text back down smoothly
        if (comboText != null && comboText.gameObject.activeSelf)
        {
            comboText.transform.localScale = Vector3.Lerp(comboText.transform.localScale, originalScale, Time.deltaTime * lerpSpeed);
        }
    }

    public void RegisterKill()
    {
        // COMBO LOGIC: Increase streak and refresh the timer
        currentStreak++;
        lastKillTime = Time.time;

        // MULTI-KILL LOGIC: Group kills happening very close together
        if (burstKillCount == 0) burstTimer = Time.time;
        burstKillCount++;

        // Add to score (Example: 10 points * your current combo)
        totalScore += (10 * currentStreak);

        UpdateScoreUI();
        UpdateComboUI();

        // Pop the combo text size
        if (comboText != null) comboText.transform.localScale = originalScale * scaleFactor;
    }

    private void HandleMultiKillAnnouncement()
    {
        // Only trigger the "TRIPLE KILL!" style text if 2+ died at once
        if (burstKillCount >= 2 && rankText != null)
        {
            rankText.gameObject.SetActive(true);
            rankText.text = GetRankName(burstKillCount);

            // Auto-hide the rank text after 1.5 seconds so it can "pop" again later
            CancelInvoke("HideRankText");
            Invoke("HideRankText", 1.5f);
        }

        burstKillCount = 0; // Reset the burst buffer
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = totalScore.ToString();
    }

    private void UpdateComboUI()
    {
        if (comboText == null) return;
        comboText.gameObject.SetActive(true);
        comboText.text = "x" + currentStreak;
    }

    private void HideRankText()
    {
        if (rankText != null) rankText.gameObject.SetActive(false);
    }

    private string GetRankName(int burst)
    {
        return burst switch
        {
            2 => "DOUBLE KILL!",
            3 => "TRIPLE KILL!",
            4 => "QUADRA KILL!",
            5 => "PENTA KILL!",
            _ => "TOTAL CARNAGE!!"
        };
    }

    public void ResetCombo()
    {
        currentStreak = 0;
        if (comboText != null) comboText.gameObject.SetActive(false);
    }
}