using System.Collections.Generic;
using UnityEngine;

public class AttachGameObjectsToParticles : MonoBehaviour
{
    public GameObject fireflyPrefab;
    public int amount = 50;

    public Vector2 areaSize = new Vector2(10, 5);

    public float moveForce = 2f;
    public float maxSpeed = 1.5f;

    private List<Rigidbody2D> fireflies = new List<Rigidbody2D>();

    void Start()
    {
        SpawnFireflies();
    }

    void SpawnFireflies()
    {
        for (int i = 0; i < amount; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                Random.Range(-areaSize.y / 2, areaSize.y / 2)
            );

            GameObject fly = Instantiate(fireflyPrefab, pos, Quaternion.identity);

            // Random starting direction
            SpriteRenderer sr = fly.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.flipX = Random.value > 0.5f;
            }

            Rigidbody2D rb = fly.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // Give them a random starting movement
                rb.AddForce(Random.insideUnitCircle * moveForce);

                fireflies.Add(rb);
            }
        }
    }

    void FixedUpdate()
    {
        foreach (Rigidbody2D rb in fireflies)
        {
            if (rb == null)
                continue;

            // Random drifting movement
            Vector2 randomDirection = Random.insideUnitCircle;

            rb.AddForce(randomDirection * moveForce);

            // Limit speed
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }

            // Face movement direction
            SpriteRenderer sr = rb.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                if (rb.linearVelocity.x > 0.05f)
                {
                    sr.flipX = true;
                }
                else if (rb.linearVelocity.x < -0.05f)
                {
                    sr.flipX = false;
                }
            }
        }
    }
}