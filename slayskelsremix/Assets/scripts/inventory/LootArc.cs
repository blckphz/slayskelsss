using UnityEngine;
using System.Collections;

public class LootArc : MonoBehaviour
{
    public float arcHeight = 1f;
    public float arcDuration = 0.5f;
    public float spreadRadius = 1.0f;

    [Header("Bounce")]
    public float bounceHeight = 0.2f;
    public float bounceDuration = 0.15f;

    [Header("Audio")]
    public AudioClip bounceClip;

    private Vector3 startPos;
    private Vector3 targetPos;

    // per-instance randomized values
    private float arcH;
    private float arcD;
    private float bounceH;
    private float bounceD;

    public void Initialize(Vector3 origin)
    {
        startPos = origin;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(0.3f, spreadRadius);

        Vector3 offset = new Vector3(randomDir.x, randomDir.y, 0f) * distance;
        targetPos = origin + offset;

        // ✨ RANDOMNESS PER ITEM
        arcH = arcHeight * Random.Range(0.7f, 1.35f);
        arcD = arcDuration * Random.Range(0.85f, 1.25f);

        bounceH = bounceHeight * Random.Range(0.6f, 1.5f);
        bounceD = bounceDuration * Random.Range(0.85f, 1.2f);

        StartCoroutine(ArcRoutine());
    }

    private IEnumerator ArcRoutine()
    {
        float time = 0f;

        while (time < arcD)
        {
            time += Time.deltaTime;
            float t = time / arcD;

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

            float height = arcH * 4f * (t - t * t);
            pos.y += height;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;

        yield return StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        if (bounceClip != null)
        {
            AudioManager.Instance.PlaySound(bounceClip, 1f);
        }

        float time = 0f;

        while (time < bounceD)
        {
            time += Time.deltaTime;
            float t = time / bounceD;

            float height = bounceH * 4f * (t - t * t);

            Vector3 pos = targetPos;
            pos.y += height;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;
    }
}