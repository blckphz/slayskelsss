using UnityEngine;
using System.Collections;

public class invUIBgPulse : MonoBehaviour
{
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.05f;

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        StartCoroutine(Pulse());
    }

    IEnumerator Pulse()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            transform.localScale = startScale + startScale * (t - 0.5f) * 2f * pulseAmount;
            yield return null;
        }
    }
}