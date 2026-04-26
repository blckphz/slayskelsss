using UnityEngine;
using System.Collections.Generic;

public partial class TurretBehaviour : MonoBehaviour
{
    public static List<TurretBehaviour> ActiveTurrets = new List<TurretBehaviour>();

    [Header("Stats")]
    [SerializeField] private float health;
    private float turretDamage;
    private float shootFrequency;
    private int pierceAmount;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("References")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    private Animator animator;
    private float shootTimer = 0f;
    private bool isActive = false;
    private bool hasPermissionToShoot = true;

    private objectHealth objHealth;
    private buildableTurret ammoController;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        objHealth = GetComponent<objectHealth>();
        ammoController = GetComponent<buildableTurret>();
    }

    public void Setup(float hp, float lifetime, Transform caster)
    {
        health = hp;
        isActive = true;
        shootTimer = 0f;
        gameObject.SetActive(true);

        if (!ActiveTurrets.Contains(this))
            ActiveTurrets.Add(this);

        CancelInvoke(nameof(Deactivate));
        if (lifetime > 0f) Invoke(nameof(Deactivate), lifetime);
    }

    public void SetCombatStats(float dmg, float freq, int pierce)
    {
        turretDamage = dmg;
        shootFrequency = freq;
        pierceAmount = pierce;
    }

    public void SetFiringPermission(bool canShoot) => hasPermissionToShoot = canShoot;

    private void Update()
    {
        if (!isActive || !hasPermissionToShoot) return;

        shootTimer += Time.deltaTime;

        if (shootTimer >= shootFrequency)
        {
            // FIX: Find target BEFORE consuming ammo
            enemyHealth target = FindNearestEnemy();

            if (target != null)
            {
                // Only drain ammo if we are actually firing
                if (ammoController != null)
                {
                    if (!ammoController.ConsumeAmmo()) return;
                }

                Debug.Log($"<color=green>[Turret Logic]</color> {gameObject.name} shooting {target.name}");
                shootTimer = 0f;
                FaceTarget(target.transform);
                Shoot(target.transform);
            }
            else if (animator != null)
            {
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
        if (projectilePrefab == null) return;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 dir = (target.position - spawnPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle + 90f));
        if (proj.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb)) rb.linearVelocity = dir * projectileSpeed;
        if (proj.TryGetComponent<Projectile>(out Projectile p)) p.SetDamage(turretDamage, pierceAmount);
    }

    public void TakeDamage(float amount)
    {
        // If this turret has an objectHealth component, damage that
        if (objHealth != null)
        {
            objHealth.TakeDamage(amount);
        }
        else
        {
            // Fallback if objectHealth is missing
            health -= amount;
            if (health <= 0) Deactivate();
        }
    }

    public void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ActiveTurrets.Remove(this);
        CancelInvoke();
    }
}