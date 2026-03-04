using UnityEngine;
using System.Collections;

public class slowmoManager : MonoBehaviour
{
    public static slowmoManager Instance;

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public static void TriggerSlowmo(float duration, float intensity)
    {
        if (Instance != null)
        {
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.SlowmoRoutine(duration, intensity));
        }
    }

    private IEnumerator SlowmoRoutine(float duration, float intensity)
    {
        // Set time scale (e.g., 0.2f for 20% speed)
        Time.timeScale = intensity;
        // Adjust fixedDeltaTime so physics don't stutter
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Wait using Realtime because Time.time is now slowed down!
        yield return new WaitForSecondsRealtime(duration);

        // Reset time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}