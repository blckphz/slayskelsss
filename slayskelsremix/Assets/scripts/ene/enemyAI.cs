using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    private IAstarAI ai;
    private Animator anim;

    [Header("Targeting Settings")]
    private Transform currentTarget;
    public float detectionRefreshRate = 0.5f;

    [Header("Combat Settings")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public float damageAmount = 10f;
    private float lastAttackTime;

    [Header("Trigger Setup")]
    [SerializeField] private CircleCollider2D weaponTrigger;
    [SerializeField] private LayerMask targetLayers;

    void Start()
    {
        ai = GetComponent<IAstarAI>();
        anim = GetComponent<Animator>();

        InvokeRepeating(nameof(FindClosestTarget), 0f, detectionRefreshRate);

        if (weaponTrigger == null)
            weaponTrigger = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        if (currentTarget == null || ai == null)
        {
            StopMovement();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        // CHECK RANGE
        if (distanceToTarget <= attackRange)
        {
            ai.isStopped = true;

            // If target is inside trigger and cooldown is over, start attacking
            if (Time.time >= lastAttackTime + attackCooldown && IsTargetInWeaponTrigger())
            {
                anim.SetBool("isattacking", true);
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // TARGET OUTSIDE RANGE: Stop attacking and resume movement
            anim.SetBool("isattacking", false);
            ai.isStopped = false;
            ai.destination = currentTarget.position;
        }

        UpdateAnimator();
    }

    void StopMovement()
    {
        ai.isStopped = true;
        anim.SetBool("isattacking", false);
    }

    private bool IsTargetInWeaponTrigger()
    {
        if (weaponTrigger == null) return false;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayers);
        filter.useLayerMask = true;
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[1];
        int hitCount = weaponTrigger.Overlap(filter, results);

        return hitCount > 0;
    }

    void UpdateAnimator()
    {
        Vector3 velocity = ai.velocity;
        if (velocity.magnitude > 0.1f)
        {
            Vector2 movementVector = new Vector2(velocity.x, velocity.y).normalized;
            anim.SetFloat("x", movementVector.x);
            anim.SetFloat("y", movementVector.y);
        }
    }

    // CALLED BY ANIMATION EVENT
    public void checkforplayerdmg()
    {
        if (weaponTrigger == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayers);
        filter.useLayerMask = true;
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];
        int hitCount = weaponTrigger.Overlap(filter, results);

        bool didHitSomething = false;

        for (int i = 0; i < hitCount; i++)
        {
            // 🔥 UNIVERSAL DAMAGE SYSTEM

            // Player
            playerHealth pHealth = results[i].GetComponent<playerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damageAmount);
                didHitSomething = true;
                continue;
            }

            // Turret
            TurretBehaviour tBehav = results[i].GetComponent<TurretBehaviour>();
            if (tBehav != null)
            {
                tBehav.TakeDamage(damageAmount);
                didHitSomething = true;
                continue;
            }

            // 🔥 ANY BUILDABLE OBJECT
            objectHealth objHealth = results[i].GetComponent<objectHealth>();
            if (objHealth != null)
            {
                objHealth.TakeDamage(damageAmount);
                didHitSomething = true;
            }
        }

        anim.SetBool("isattacking", false);

        if (!didHitSomething)
        {
            slowmoManager.TriggerSlowmo(0.5f, 0.2f);
        }
    }

    private void FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        // ---------------- PLAYER ----------------
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = player.transform;
            }
        }

        // ---------------- TURRETS ----------------
        TurretBehaviour[] turrets = FindObjectsOfType<TurretBehaviour>();
        foreach (TurretBehaviour turret in turrets)
        {
            if (!turret.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, turret.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = turret.transform;
            }
        }

        // ---------------- BUILDINGS (🔥 NEW) ----------------
        objectHealth[] objects = FindObjectsOfType<objectHealth>();
        foreach (objectHealth obj in objects)
        {
            if (!obj.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = obj.transform;
            }
        }

        currentTarget = bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (weaponTrigger != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + (Vector3)weaponTrigger.offset, weaponTrigger.radius);
        }
    }
}