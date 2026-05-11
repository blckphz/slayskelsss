using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GrassSway : MonoBehaviour
{
    public float swayAmount = 0.1f;   // world units to sway
    public float swaySpeed = 6f;      // how fast the sway oscillates
    public float swayDecay = 3f;      // how fast it stops
    public int pixelsPerUnit = 32;    // match your sprite import setting

    Vector3 startPos;
    Coroutine swayRoutine;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (swayRoutine != null) StopCoroutine(swayRoutine);
            swayRoutine = StartCoroutine(Sway());
        }
    }

    System.Collections.IEnumerator Sway()
    {
        float time = 0f;
        while (true)
        {
            float decay = Mathf.Exp(-swayDecay * time);
            if (decay < 0.01f) break;

            float offset = Mathf.Sin(time * swaySpeed) * swayAmount * decay;
            Vector3 targetPos = startPos + new Vector3(offset, 0f, 0f);

            // Snap to pixel grid for crispness
            targetPos.x = Mathf.Round(targetPos.x * pixelsPerUnit) / pixelsPerUnit;
            targetPos.y = Mathf.Round(targetPos.y * pixelsPerUnit) / pixelsPerUnit;

            transform.localPosition = targetPos;

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = startPos;
        swayRoutine = null;
    }
}
