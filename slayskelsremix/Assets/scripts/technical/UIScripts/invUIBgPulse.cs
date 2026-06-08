using UnityEngine;
using System.Collections;

public class invUIBgPulse : MonoBehaviour
{
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.05f;

    private Vector3 startScale;
    private Coroutine routine;
    private float offset;

    void Awake()
    {
        startScale = transform.localScale;
        offset = Random.Range(0f, 10f);
    }

    public void SetPulsing(bool state)
    {
        if (state)
        {
            if (routine == null)
                routine = StartCoroutine(Pulse());
        }
        else
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            transform.localScale = startScale;
        }
    }

    IEnumerator Pulse()
    {
        float time = 0f;

        while (true)
        {
            time += Time.unscaledDeltaTime;

            float t = (Mathf.Sin((time + offset) * pulseSpeed) + 1f) * 0.5f;

            transform.localScale =
                startScale + startScale * (t - 0.5f) * 2f * pulseAmount;

            yield return null;
        }
    }
}