using System.Collections.Generic;
using UnityEngine;

public class grenadeBehav : MonoBehaviour
{
    private float damage;
    public float radius;
    public float fuseTime = 2.5f;
    public GameObject explosionEffect;

    [Header("Synergy & Physics")]
    public int chargesPerEnemyHit = 1;
    public float implosionForce = 2f;

    private bool hasExploded = false;

    public static grenadeBehav ActiveGrenade;
    public static grenadeSO ActiveGrenadeSO;

    public void Initialize(float dmg, float rad, float fuse = 2.5f)
    {
        damage = dmg;
        radius = rad;
        fuseTime = fuse;
        hasExploded = false;

        ActiveGrenade = this;
        Invoke(nameof(Explode), fuseTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;
        if (collision.GetComponent<IDamageable>() != null)
        {
            Explode();
        }
    }

    public void ManualExplode() => Explode();

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        CancelInvoke(nameof(Explode));

        HashSet<IDamageable> uniqueEnemiesHit = new HashSet<IDamageable>();
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D obj in objectsInRange)
        {
            IDamageable target = obj.GetComponent<IDamageable>();
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

            if (target != null && !uniqueEnemiesHit.Contains(target))
            {
                uniqueEnemiesHit.Add(target);
                chainController.hitCcunter += chargesPerEnemyHit;
                target.TakeDamage(damage);
            }

            // --- IMPROVED PHYSICS HANDLING ---
            if (rb != null)
            {
                Vector2 pullDir = (Vector2)transform.position - (Vector2)obj.transform.position;
                Vector2 finalForce = pullDir.normalized * implosionForce;

                // Check if it's an enemy (AI controlled) or just a prop/gold
                enemyHealth enemy = obj.GetComponent<enemyHealth>();
                if (enemy != null)
                {
                    enemy.ApplyImpulse(finalForce);
                }
                else
                {
                    // Regular physics for gold, crates, etc.
                    rb.AddForce(finalForce, ForceMode2D.Impulse);
                }
            }
        }

        if (explosionEffect) Instantiate(explosionEffect, transform.position, Quaternion.identity);
        if (ActiveGrenade == this) ActiveGrenade = null;
        Destroy(gameObject);
    }
}