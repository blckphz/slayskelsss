using UnityEngine;

public class grenadeBehav : MonoBehaviour
{
    private float damage;
    private float radius;
    public float fuseTime = 2.5f;
    public GameObject explosionEffect;

    private bool hasExploded = false;

    // Static references
    public static grenadeBehav ActiveGrenade;
    public static grenadeSO ActiveGrenadeSO;

    public void Initialize(float dmg, float rad, float fuse = 2.5f)
    {
        damage = dmg;
        radius = rad;
        fuseTime = fuse;
        hasExploded = false;

        // Register this specific instance as THE active grenade
        ActiveGrenade = this;
        Debug.Log($"[Grenade] {gameObject.name} Initialized. Static reference set.");

        Invoke(nameof(Explode), fuseTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        // Check if we hit something damageable (Impact Detonation)
        if (collision.GetComponent<IDamageable>() != null)
        {
            Debug.Log("[Grenade] Impact Detonation via OnTriggerEnter2D.");
            Explode();
        }
    }

    public void ManualExplode()
    {
        Debug.Log("[Grenade] ManualExplode signal received.");
        if (hasExploded)
        {
            Debug.LogWarning("[Grenade] ManualExplode failed: Already exploded.");
            return;
        }
        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log("[Grenade] Exploding now!");
        CancelInvoke(nameof(Explode));

        // AoE Damage logic
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D obj in objectsInRange)
        {
            IDamageable target = obj.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (Vector2)obj.transform.position - (Vector2)transform.position;
                rb.AddForce(dir.normalized * 5f, ForceMode2D.Impulse);
            }
        }

        if (explosionEffect)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // IMPORTANT: Clear the static reference so we can throw another one
        if (ActiveGrenade == this)
        {
            Debug.Log("[Grenade] Clearing ActiveGrenade static reference.");
            ActiveGrenade = null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}