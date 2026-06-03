using UnityEngine;
using Unity.Cinemachine;
using Diagnostics = System.Diagnostics;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    private CinemachineBasicMultiChannelPerlin perlin;

    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    [Header("Debug")]
    public bool debugShakeCalls = true;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        // get Cinemachine camera
        var cam = GetComponent<CinemachineCamera>();

        if (cam != null)
        {
            perlin = cam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        if (perlin == null)
        {
            UnityEngine.Debug.LogError(
                "CameraShaker: No CinemachineBasicMultiChannelPerlin found on this CinemachineCamera!"
            );
        }
    }

    private void Update()
    {
        if (perlin == null)
            return;

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            float t = shakeTimer / shakeTimerTotal;

            perlin.AmplitudeGain =
                Mathf.Lerp(0f, startingIntensity, t);
        }
        else
        {
            perlin.AmplitudeGain = 0f;
        }
    }

    // =====================================================
    // SHAKE
    // =====================================================

    public void Shake(float intensity, float duration = 0.15f)
    {
        if (perlin == null)
        {
            UnityEngine.Debug.LogWarning(
                "[CameraShaker] Shake requested but no Perlin noise component found."
            );
            return;
        }

        // ================= DEBUG LOG =================

        if (debugShakeCalls)
        {
            Diagnostics.StackTrace trace =
                new Diagnostics.StackTrace();

            string caller = "Unknown Caller";

            // frame 1 is usually the direct caller
            if (trace.FrameCount > 1)
            {
                var frame = trace.GetFrame(1);

                if (frame != null)
                {
                    var method = frame.GetMethod();

                    if (method != null)
                    {
                        caller =
                            $"{method.DeclaringType.Name}.{method.Name}";
                    }
                }
            }

            /*

            UnityEngine.Debug.Log(
                $"[CameraShaker] SHAKE REQUESTED " +
                $"| Intensity: {intensity} " +
                $"| Duration: {duration} " +
                $"| From: {caller}"
            );
            */

        }

        // ================= APPLY SHAKE =================

        startingIntensity = intensity;
        shakeTimerTotal = duration;
        shakeTimer = duration;

        perlin.AmplitudeGain = intensity;
    }
}