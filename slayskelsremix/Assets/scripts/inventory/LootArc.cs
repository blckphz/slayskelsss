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

    private Vector3 startPos;
    private Vector3 targetPos;

    public void Initialize(Vector3 origin)
    {
        startPos = origin;

        // Generate a random direction outward
        Vector2 randomDir = Random.insideUnitCircle.normalized;

        // Distance from origin
        float distance = Random.Range(0.3f, spreadRadius);

        Vector3 offset = new Vector3(randomDir.x, randomDir.y, 0f) * distance;

        targetPos = origin + offset;

        StartCoroutine(ArcRoutine());
    }

    private IEnumerator ArcRoutine()
    {
        float time = 0f;

        while (time < arcDuration)
        {
            time += Time.deltaTime;
            float t = time / arcDuration;

            // Base linear interpolation
            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

            // Arc height (parabolic curve)
            float height = arcHeight * 4f * (t - t * t);
            pos.y += height;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;

        yield return StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        float time = 0f;

        while (time < bounceDuration)
        {
            time += Time.deltaTime;
            float t = time / bounceDuration;

            float height = bounceHeight * 4f * (t - t * t);

            Vector3 pos = targetPos;
            pos.y += height;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;
    }
}