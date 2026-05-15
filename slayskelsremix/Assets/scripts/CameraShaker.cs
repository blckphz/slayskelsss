using UnityEngine;
using Unity.Cinemachine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        var cam = GetComponent<CinemachineCamera>();
        if (cam != null)
        {
            // Specifically looking for the Noise component on the Virtual Camera
            perlin = cam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        if (perlin == null)
        {
            Debug.LogError("CameraShaker: No CinemachineBasicMultiChannelPerlin found on this CinemachineCamera!");
        }
    }

    void Update()
    {
        if (perlin == null) return;

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            float t = shakeTimer / shakeTimerTotal;
            perlin.AmplitudeGain = Mathf.Lerp(0f, startingIntensity, t);
        }
        else
        {
            perlin.AmplitudeGain = 0f;
        }
    }

    public void Shake(float intensity, float duration = 0.15f)
    {
        if (perlin == null) return;

        startingIntensity = intensity;
        shakeTimerTotal = duration;
        shakeTimer = duration;

        perlin.AmplitudeGain = intensity;
    }
}