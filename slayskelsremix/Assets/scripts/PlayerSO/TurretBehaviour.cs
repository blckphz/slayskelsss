using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    // Turret stats
    private float hp;
    private float lifetime;
    private Transform owner;

    private float damage;
    public float shootFrequency; // seconds between shots
    public float projectileSpeed = 10f;
    public float detectionRadius = 10f;

    public GameObject projectilePrefab;

    // Internal timer
    private float shootTimer = 0f;
    private bool isActive = false;

    // ✅ ADDED
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Setup turret when spawned
    public void Setup(float health, float life, Transform caster)
    {
        hp = health;
        lifetime = life;
        owner = caster;

        gameObject.SetActive(true);
        isActive = true;
        shootTimer = 0f;

        Debug.Log($"[Turret] Activated with {hp} HP, lifetime {lifetime}s");

        // Start lifetime countdown
        Invoke(nameof(Deactivate), lifetime);
    }

    public void SetCombatStats(float dmg, float freq)
    {
        damage = dmg;
        Debug.Log($"[Turret] Combat stats set: Damage={damage}, ShootFrequency={shootFrequency}s");
    }

    private void Update()
    {
        if (!isActive) return;

        shootTimer += Time.deltaTime;

        if (shootTimer >= shootFrequency)
        {
            shootTimer = 0f;

            enemyHealth target = FindNearestEnemy();
            if (target != null)
            {
                // ✅ ADDED (face target using animator)
                FaceTarget(target.transform);

                Shoot(target.transform);
            }
            else
            {

                // Optional: reset direction when no enemy
                animator.SetFloat("x", 0f);
                animator.SetFloat("y", 0f);
            }
        }
    }

    private void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
        Debug.Log("[Turret] Lifetime ended, turret deactivated.");
    }

    // ✅ ADDED METHOD
    void FaceTarget(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;

        animator.SetFloat("x", dir.x);
        animator.SetFloat("y", dir.y);
    }

    // Find nearest enemy within detection radius
    enemyHealth FindNearestEnemy()
    {
        enemyHealth[] enemies = FindObjectsOfType<enemyHealth>();
        enemyHealth nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= detectionRadius)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }

    // Shoot a projectile at a target
    void Shoot(Transform target)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[Turret] Projectile prefab missing!");
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + 90f);

        GameObject proj = Instantiate(projectilePrefab, transform.position, rotation);

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }

        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDamage(damage);
        }

        Debug.Log($"[Turret] Shot fired at {target.name} with damage {damage}");
    }
}
