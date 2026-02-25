using System.Collections.Generic;
using UnityEngine;

public class chainlightningBehav : MonoBehaviour
{
    private float damage;
    private int bouncesRemaining;
    private float radius;
    private float rotationOffset;
    private bool istickdmg;
    private float slowduration, sloweffectivenes;

    // We use this to prevent hitting the same enemy twice in one shot
    private List<GameObject> hitEnemies = new List<GameObject>();
    private Rigidbody2D rb;
    private Transform spriteTransform;

    // Timer for auto-deactivation
    private Coroutine deactivationRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Find the sprite transform safely
        spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
    }

    public void Setup(float dmg, int bounces, float rad, Vector2 velocity, float rotOffset, float stunDuration, float stuneffect, bool isapplyingtickdmg)
    {
        // 1. Reset state (CRITICAL for pooling)
        hitEnemies.Clear();
        damage = dmg;
        bouncesRemaining = bounces;
        radius = rad;
        rotationOffset = rotOffset;
        slowduration = stunDuration;
        sloweffectivenes = stuneffect;
        istickdmg = isapplyingtickdmg;


        // 2. Physics setup
        rb.linearVelocity = velocity;
        RotateSprite(velocity);

        // 3. Instead of Destroy, start a "Return to Pool" timer
        if (deactivationRoutine != null) StopCoroutine(deactivationRoutine);
        deactivationRoutine = StartCoroutine(DeactivateAfterTime(5f));
    }

    private System.Collections.IEnumerator DeactivateAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        Deactivate();
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            RotateSprite(rb.linearVelocity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null && !hitEnemies.Contains(collision.gameObject))
        {
            target.TakeDamage(damage);
            target.ApplySlow(sloweffectivenes, slowduration); // 40% slow for 2 seconds
            hitEnemies.Add(collision.gameObject);

            if (bouncesRemaining > 0)
                Bounce();
            else
                Deactivate();
        }
    }

    void Bounce()
    {
        bouncesRemaining--;

        GameObject closest = FindNextTarget();
        if (closest == null)
        {
            Deactivate();
            return;
        }

        Vector2 direction = ((Vector2)closest.transform.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * rb.linearVelocity.magnitude;
        RotateSprite(direction);
    }

    void RotateSprite(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        spriteTransform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    GameObject FindNextTarget()
    {
        Collider2D[] candidates = Physics2D.OverlapCircleAll(transform.position, radius);

        GameObject bestTarget = null;
        float closestDist = Mathf.Infinity;

        foreach (var col in candidates)
        {
            // Only target things with health that we haven't hit yet
            if (col.GetComponent<IDamageable>() != null && !hitEnemies.Contains(col.gameObject))
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestTarget = col.gameObject;
                }
            }
        }
        return bestTarget;
    }

    void Deactivate()
    {
        // Return to pool
        gameObject.SetActive(false);
    }
}