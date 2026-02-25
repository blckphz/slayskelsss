using UnityEngine;
using Unity.Cinemachine;

public class CameraShaker : MonoBehaviour
{
    private static CinemachineBasicMultiChannelPerlin perlin;
    private static float shakeTimer;
    private static float shakeTimerTotal;
    private static float startingIntensity;

    void Awake()
    {
        // Grab the Cinemachine Camera on this object
        var cam = GetComponent<CinemachineCamera>();
        perlin = cam.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            // Fade out
            float t = shakeTimer / shakeTimerTotal;
            perlin.AmplitudeGain = Mathf.Lerp(0f, startingIntensity, t);
        }
        else if (perlin != null)
        {
            perlin.AmplitudeGain = 0f;
        }
    }

    /// <summary>
    /// Call this from anywhere: CameraShaker.Shake(intensity, duration);
    /// </summary>
    public static void Shake(float intensity, float duration = 0.15f)
    {
        if (perlin == null) return;

        startingIntensity = intensity;
        shakeTimerTotal = duration;
        shakeTimer = duration;

        perlin.AmplitudeGain = intensity;
    }
}
