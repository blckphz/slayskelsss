using UnityEngine;

public class BloodMoon : MonoBehaviour
{
    [Header("References")]
    public DayNightCycle dayNightCycle;

    [Header("Blood Moon Settings")]
    public Color bloodMoonTint = new Color(1f, 0.3f, 0.3f);
    public float intensityMultiplier = 1.2f;

    void Start()
    {
        if (dayNightCycle == null)
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
    }

    public Color GetModifiedColor(Color baseColor)
    {
        if (dayNightCycle != null && dayNightCycle.IsBloodMoon)
            return Color.Lerp(baseColor, bloodMoonTint, 0.5f);

        return baseColor;
    }

    public float GetModifiedIntensity(float baseIntensity)
    {
        if (dayNightCycle != null && dayNightCycle.IsBloodMoon)
            return baseIntensity * intensityMultiplier;

        return baseIntensity;
    }
}