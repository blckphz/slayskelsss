using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 60f;

    [Header("Time Speed")]
    public float normalSpeed = 1f;
    public float sleepSpeed = 20f;

    private float currentSpeed = 1f;

    [Header("References")]
    public Light2D globalLight;
    public Gradient nightDayColor;
    public AnimationCurve intensityCurve;

    [Header("UI")]
    public TextMeshProUGUI timeText;

    [Header("Blood Moon")]
    public int bloodMoonEveryNDays = 5;
    public Color bloodMoonTint = new Color(1f, 0.3f, 0.3f);
    public float bloodMoonIntensityMultiplier = 1.2f;

    [Header("Blood Moon Transition")]
    public float bloodMoonFadeSpeed = 0.5f;

    private float bloodMoonBlend = 0f;

    public float TotalTime { get; private set; }
    public float RawTime { get; private set; }
    public int DaysPassed { get; private set; }

    public bool IsBloodMoon =>
        DaysPassed > 0 &&
        DaysPassed % bloodMoonEveryNDays == 0;

    void Start()
    {
        currentSpeed = normalSpeed;
    }

    void Update()
    {
        UpdateTime();
        ApplyLighting();
        UpdateUI();
    }

    private void UpdateTime()
    {
        float delta = (Time.deltaTime * currentSpeed) / dayDuration;

        RawTime += delta;
        TotalTime += delta;

        if (RawTime >= 1f)
        {
            RawTime -= 1f;
            DaysPassed++;
        }
    }

    public void StartSleeping()
    {
        currentSpeed = sleepSpeed;
    }

    public void StopSleeping()
    {
        currentSpeed = normalSpeed;
    }

    public float GetHour()
    {
        return RawTime * 24f;
    }

    public bool IsMorning()
    {
        float hour = GetHour();
        // Strictly between 6:00 AM (06:00) and 12:00 PM (Noon)
        return hour >= 6f && hour < 12f;
    }

    public bool IsNight()
    {
        float hour = GetHour();
        // Between 6:00 PM (18:00) and 6:00 AM (06:00)
        return hour >= 18f || hour < 6f;
    }

    private void ApplyLighting()
    {
        if (globalLight == null)
            return;

        bool isNight = IsNight();

        Color baseColor = nightDayColor.Evaluate(RawTime);
        float baseIntensity = intensityCurve.Evaluate(RawTime);

        float targetBlend = (IsBloodMoon && isNight) ? 1f : 0f;

        bloodMoonBlend = Mathf.MoveTowards(
            bloodMoonBlend,
            targetBlend,
            bloodMoonFadeSpeed * Time.deltaTime
        );

        baseColor = Color.Lerp(
            baseColor,
            bloodMoonTint,
            bloodMoonBlend * 0.5f
        );

        baseIntensity *= Mathf.Lerp(
            1f,
            bloodMoonIntensityMultiplier,
            bloodMoonBlend
        );

        globalLight.color = baseColor;
        globalLight.intensity = baseIntensity;
    }

    private void UpdateUI()
    {
        if (timeText == null)
            return;

        int totalHours = Mathf.FloorToInt(RawTime * 24f);
        int hour12 = totalHours % 12;

        if (hour12 == 0)
            hour12 = 12;

        string period = totalHours >= 12 ? "PM" : "AM";

        timeText.text = $"Day {DaysPassed + 1} {hour12:00} {period}";

        timeText.color = Color.Lerp(
            Color.white,
            bloodMoonTint,
            bloodMoonBlend * 0.5f
        );
    }

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