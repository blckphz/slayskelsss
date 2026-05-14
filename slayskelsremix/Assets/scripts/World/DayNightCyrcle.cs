using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 60f;
    public float startTimePercent = 0.25f;

    [Header("References")]
    public Light2D globalLight;
    public Gradient nightDayColor;
    public AnimationCurve intensityCurve;

    [Header("UI")]
    public TextMeshProUGUI timeText;

    public float TotalTime { get; private set; }
    private float _rawTime;
    public int DaysPassed { get; private set; }

    void Start()
    {
        Debug.Log("[DayNightCycle] Starting...");
        LoadGame();
    }

    void Update()
    {
        UpdateTime();
        ApplyLighting();
        UpdateUI();
    }

    private void UpdateTime()
    {
        float delta = Time.deltaTime / dayDuration;

        _rawTime += delta;
        TotalTime += delta;

        if (_rawTime >= 1f)
        {
            _rawTime -= 1f;
            DaysPassed++;

            Debug.Log($"[DayNightCycle] New Day Started! Days Passed: {DaysPassed}");
        }
    }

    private void ApplyLighting()
    {
        if (globalLight != null)
        {
            globalLight.color = nightDayColor.Evaluate(_rawTime);
            globalLight.intensity = intensityCurve.Evaluate(_rawTime);
        }
        else
        {
            Debug.LogWarning("[DayNightCycle] Global Light reference is missing!");
        }
    }

    private void UpdateUI()
    {
        if (timeText != null)
        {
            // Convert _rawTime (0–1) into 24-hour clock
            int hours = Mathf.FloorToInt(_rawTime * 24f);
            int minutes = Mathf.FloorToInt((_rawTime * 24f - hours) * 60f);

            timeText.text = $"Day {DaysPassed + 1}\n{hours:00}:{minutes:00}";
        }
    }

    public void SaveGame()
    {
        PlayerPrefs.SetFloat("TotalTime", TotalTime);
        PlayerPrefs.SetFloat("RawTime", _rawTime);
        PlayerPrefs.SetInt("DaysPassed", DaysPassed);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        TotalTime = PlayerPrefs.GetFloat("TotalTime", startTimePercent);
        _rawTime = PlayerPrefs.GetFloat("RawTime", startTimePercent);
        DaysPassed = PlayerPrefs.GetInt("DaysPassed", 0);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}