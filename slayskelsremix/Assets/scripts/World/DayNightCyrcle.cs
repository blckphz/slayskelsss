using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 60f;
    public float startTimePercent = 0.25f;

    [Header("References")]
    public Light2D globalLight;
    public Gradient nightDayColor;
    public AnimationCurve intensityCurve;

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
    }

    private void UpdateTime()
    {
        // delta is the percentage of a day that passed this frame
        float delta = Time.deltaTime / dayDuration;

        _rawTime += delta;
        TotalTime += delta;

        // Debug current time occasionally (optional)
        // Debug.Log($"[DayNightCycle] RawTime: {_rawTime:F3} | TotalTime: {TotalTime:F3}");

        if (_rawTime >= 1f)
        {
            _rawTime -= 1f;
            DaysPassed++;

            Debug.Log($"[DayNightCycle] New Day Started! Days Passed: {DaysPassed}");
            Debug.Log($"[DayNightCycle] RawTime reset to {_rawTime:F3}, TotalTime: {TotalTime:F3}");
        }
    }

    private void ApplyLighting()
    {
        if (globalLight != null)
        {
            globalLight.color = nightDayColor.Evaluate(_rawTime);
            globalLight.intensity = intensityCurve.Evaluate(_rawTime);

            // Uncomment if you want live lighting debug (can spam console)
            // Debug.Log($"[DayNightCycle] Light Intensity: {globalLight.intensity:F2}");
        }
        else
        {
            Debug.LogWarning("[DayNightCycle] Global Light reference is missing!");
        }
    }

    public void SaveGame()
    {
        PlayerPrefs.SetFloat("TotalTime", TotalTime);
        PlayerPrefs.SetFloat("RawTime", _rawTime);
        PlayerPrefs.SetInt("DaysPassed", DaysPassed);
        PlayerPrefs.Save();

        Debug.Log(
            $"[DayNightCycle] Game Saved!\n" +
            $"TotalTime: {TotalTime:F3}\n" +
            $"RawTime: {_rawTime:F3}\n" +
            $"DaysPassed: {DaysPassed}"
        );
    }

    public void LoadGame()
    {
        TotalTime = PlayerPrefs.GetFloat("TotalTime", startTimePercent);
        _rawTime = PlayerPrefs.GetFloat("RawTime", startTimePercent);
        DaysPassed = PlayerPrefs.GetInt("DaysPassed", 0);

        Debug.Log(
            $"[DayNightCycle] Game Loaded!\n" +
            $"TotalTime: {TotalTime:F3}\n" +
            $"RawTime: {_rawTime:F3}\n" +
            $"DaysPassed: {DaysPassed}"
        );
    }

    private void OnApplicationQuit()
    {
        Debug.Log("[DayNightCycle] Application quitting, saving...");
        SaveGame();
    }
}