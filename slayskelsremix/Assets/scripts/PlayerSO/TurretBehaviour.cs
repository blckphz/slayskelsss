using UnityEngine;
using System.Collections.Generic;

public partial class TurretBehaviour : MonoBehaviour
{
    // Static list that all turrets share to track who is active
    public static List<TurretBehaviour> ActiveTurrets = new List<TurretBehaviour>();

    [Header("Stats")]
    [SerializeField] private float health;
    private float maxHealth;
    private float turretDamage;
    private float shootFrequency;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float projectileSpeed = 10f;
    private Transform creator;

    [Header("References")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    private Animator animator;
    private float shootTimer = 0f;
    private bool isActive = false;

    [Header("Effects")]
    [Tooltip("Prefab with a Particle System that plays on Spawn")]
    public GameObject placementEffectPrefab;
    [Tooltip("Optional: Prefab that plays when the turret is destroyed")]
    public GameObject deathEffectPrefab;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Initializes the turret when it is spawned or pulled from a pool.
    /// </summary>
    public void Setup(float hp, float lifetime, Transform caster)
    {
        health = hp;
        maxHealth = hp;
        creator = caster;
        isActive = true;
        shootTimer = 0f;

        gameObject.SetActive(true);

        // --- PARTICLE LOGIC ---
        if (placementEffectPrefab != null)
        {
            // Spawn the particles at the turret's position
            Instantiate(placementEffectPrefab, transform.position, Quaternion.identity);
        }

        // Register this turret as active
        if (!ActiveTurrets.Contains(this))
        {
            ActiveTurrets.Add(this);
        }

        // Auto-deactivate after lifetime ends
        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), lifetime);
    }

    public void SetCombatStats(float dmg, float freq)
    {
        turretDamage = dmg;
        shootFrequency = freq;
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
                FaceTarget(target.transform);
                Shoot(target.transform);
            }
            else if (animator != null)
            {
                // Reset animator parameters if no enemies are in range
                animator.SetFloat("x", 0f);
                animator.SetFloat("y", 0f);
            }
        }
    }

    void FaceTarget(Transform target)
    {
        if (animator == null) return;
        Vector2 dir = (target.position - transform.position).normalized;
        animator.SetFloat("x", dir.x);
        animator.SetFloat("y", dir.y);
    }

    enemyHealth FindNearestEnemy()
    {
        enemyHealth[] enemies = Object.FindObjectsByType<enemyHealth>(FindObjectsSortMode.None);
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

    void Shoot(Transform target)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[Turret] No projectile prefab assigned!");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 dir = (target.position - spawnPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + 90f);
        GameObject proj = Instantiate(projectilePrefab, spawnPos, rotation);

        if (proj.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = dir * projectileSpeed;
        }

        if (proj.TryGetComponent<Projectile>(out Projectile projScript))
        {
            projScript.SetDamage(turretDamage);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        if (isActive && deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        isActive = false;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ActiveTurrets.Remove(this);
        CancelInvoke();
    }
}