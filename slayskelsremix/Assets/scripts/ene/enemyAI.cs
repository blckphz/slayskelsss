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
    public int damageAmount = 10;
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

        if (distanceToTarget <= attackRange)
        {
            ai.isStopped = true;

            if (Time.time >= lastAttackTime + attackCooldown && IsTargetInWeaponTrigger())
            {
                anim.SetBool("isattacking", true);
                lastAttackTime = Time.time;
            }
        }
        else
        {
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
            // NEW: Centralized check. Any script inheriting healthMaster works.
            healthMaster h = results[i].GetComponent<healthMaster>();
            if (h != null)
            {
                h.TakeDamage(damageAmount);
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

        // Find all objects with health
        healthMaster[] allTargets = FindObjectsOfType<healthMaster>();

        foreach (healthMaster target in allTargets)
        {
            // Don't target yourself or other enemies
            if (target is enemyHealth || !target.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = target.transform;
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