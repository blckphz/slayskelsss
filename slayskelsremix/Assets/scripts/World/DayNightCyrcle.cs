using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 60f;
    [Range(0, 1)] public float startTimePercent = 0.25f;

    [Header("References")]
    public Light2D globalLight;
    public Gradient nightDayColor;
    public AnimationCurve intensityCurve;

    public int DaysPassed { get; private set; }
    public float Hour { get; private set; }
    public float Minutes { get; private set; }

    private float _rawTime;

    void Start()
    {
        // Load data if it exists, otherwise use start percent
        LoadGame();
    }

    void Update()
    {
        UpdateTime();
        ApplyLighting();

        // Optional: Save every time a new day starts
    }

    private void UpdateTime()
    {
        _rawTime += Time.deltaTime / dayDuration;

        if (_rawTime >= 1f)
        {
            _rawTime = 0f;
            DaysPassed++;
            SaveGame(); // Auto-save on new day
        }

        float totalHours = _rawTime * 24f;
        Hour = Mathf.Floor(totalHours);
        Minutes = Mathf.Floor((totalHours - Hour) * 60f);
    }

    private void ApplyLighting()
    {
        if (globalLight != null)
        {
            globalLight.color = nightDayColor.Evaluate(_rawTime);
            globalLight.intensity = intensityCurve.Evaluate(_rawTime);
        }
    }

    // --- SAVE AND LOAD LOGIC ---

    public void SaveGame()
    {
        PlayerPrefs.SetFloat("TimeOfDay", _rawTime);
        PlayerPrefs.SetInt("DaysPassed", DaysPassed);
        PlayerPrefs.Save(); // Writes to disk
        Debug.Log("Game Saved: Day " + DaysPassed + " Time " + GetTimeString());
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("TimeOfDay"))
        {
            _rawTime = PlayerPrefs.GetFloat("TimeOfDay");
            DaysPassed = PlayerPrefs.GetInt("DaysPassed");
        }
        else
        {
            _rawTime = startTimePercent;
            DaysPassed = 0;
        }
    }

    // Optional: Reset progress
    public void ResetTime()
    {
        PlayerPrefs.DeleteKey("TimeOfDay");
        PlayerPrefs.DeleteKey("DaysPassed");
        _rawTime = startTimePercent;
        DaysPassed = 0;
    }

    public string GetTimeString() => $"{Hour:00}:{Minutes:00}";

    // Automatically saves when the player quits the game
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}