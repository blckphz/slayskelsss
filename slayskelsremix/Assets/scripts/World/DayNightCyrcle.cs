using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 60f;

    [Header("References")]
    public Light2D globalLight;
    public Gradient nightDayColor;
    public AnimationCurve intensityCurve;

    [Header("UI")]
    public TextMeshProUGUI timeText;

    public float TotalTime { get; private set; }
    public float RawTime { get; private set; }
    public int DaysPassed { get; private set; }

    void Update()
    {
        UpdateTime();
        ApplyLighting();
        UpdateUI();
    }

    private void UpdateTime()
    {
        float delta = Time.deltaTime / dayDuration;

        RawTime += delta;
        TotalTime += delta;

        if (RawTime >= 1f)
        {
            RawTime -= 1f;
            DaysPassed++;
        }
    }

    private void ApplyLighting()
    {
        if (globalLight == null) return;

        globalLight.color = nightDayColor.Evaluate(RawTime);
        globalLight.intensity = intensityCurve.Evaluate(RawTime);
    }

    private void UpdateUI()
    {
        if (timeText == null) return;

        int totalHours = Mathf.FloorToInt(RawTime * 24f);

        int hour12 = totalHours % 12;
        if (hour12 == 0) hour12 = 12;

        string period = totalHours >= 12 ? "PM" : "AM";

        timeText.text = $"Day {DaysPassed + 1} {hour12:00} {period}";
    }

    // Called by your save system
    public void LoadTime(float total, float raw, int days)
    {
        TotalTime = total;
        RawTime = raw;
        DaysPassed = days;
    }

    public void GetTime(out float total, out float raw, out int days)
    {
        total = TotalTime;
        raw = RawTime;
        days = DaysPassed;
    }
}