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

    void Start() => LoadGame();

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

        if (_rawTime >= 1f)
        {
            _rawTime -= 1f;
            DaysPassed++;
        }
    }

    private void ApplyLighting()
    {
        if (globalLight != null)
        {
            globalLight.color = nightDayColor.Evaluate(_rawTime);
            globalLight.intensity = intensityCurve.Evaluate(_rawTime);
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

    private void OnApplicationQuit() => SaveGame();
}