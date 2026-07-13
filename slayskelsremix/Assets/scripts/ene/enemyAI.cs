using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    private IAstarAI ai;
    private Animator anim;

    [Header("Target")]
    [SerializeField] private Transform currentTarget;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float loseTargetRange = 7f;

    [Header("Wandering")]
    public float wanderRadius = 3f;
    public float wanderInterval = 3f;

    private float nextWanderTime;
    private bool isChasing;
    private Vector3 spawnPosition;

    [Header("Combat Settings")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public int damageAmount = 10;

    private float lastAttackTime;

    [Header("Trigger Setup")]
    [SerializeField] private CircleCollider2D weaponTrigger;
    [SerializeField] private LayerMask targetLayers;

    private void Start()
    {
        ai = GetComponent<IAstarAI>();
        anim = GetComponent<Animator>();

        if (weaponTrigger == null)
            weaponTrigger = GetComponent<CircleCollider2D>();

        spawnPosition = transform.position;

        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            currentTarget = player.transform;
    }

    private void Update()
    {
        if (ai == null)
            return;

        if (currentTarget == null)
        {
            Wander();
            UpdateAnimator();
            return;
        }

        float distanceToTarget =
            Vector2.Distance(transform.position, currentTarget.position);

        // Start chasing when player enters detection range
        if (!isChasing && distanceToTarget <= detectionRange)
        {
            isChasing = true;
        }

        // Stop chasing when player gets too far away
        if (isChasing && distanceToTarget > loseTargetRange)
        {
            isChasing = false;
            anim.SetBool("isattacking", false);
        }

        if (isChasing)
        {
            ChaseAndAttack(distanceToTarget);
        }
        else
        {
            Wander();
        }

        UpdateAnimator();
    }

    private void ChaseAndAttack(float distanceToTarget)
    {
        if (distanceToTarget <= attackRange)
        {
            ai.isStopped = true;

            if (Time.time >= lastAttackTime + attackCooldown &&
                IsTargetInWeaponTrigger())
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
    }

    private void Wander()
    {
        anim.SetBool("isattacking", false);

        if (Time.time < nextWanderTime)
            return;

        nextWanderTime = Time.time + wanderInterval;

        Vector2 randomPoint =
            Random.insideUnitCircle * wanderRadius;

        Vector3 destination =
            spawnPosition + new Vector3(randomPoint.x, randomPoint.y, 0);

        ai.isStopped = false;
        ai.destination = destination;
    }

    private void StopMovement()
    {
        if (ai != null)
            ai.isStopped = true;

        anim.SetBool("isattacking", false);
    }

    // Public method called by enemyHealth when damage is taken
    public void CancelAttack()
    {
        if (anim != null)
        {
            anim.SetBool("isattacking", false);
            // If your animator relies on a trigger or state override to break out immediately:
            // anim.Play("Idle"); // Optional fallback line if your transitions aren't instant
        }
    }

    private bool IsTargetInWeaponTrigger()
    {
        if (weaponTrigger == null)
            return false;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayers);
        filter.useLayerMask = true;
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[1];
        int hitCount = weaponTrigger.Overlap(filter, results);

        return hitCount > 0;
    }

    private void UpdateAnimator()
    {
        if (ai == null)
            return;

        Vector3 velocity = ai.velocity;

        if (velocity.magnitude > 0.1f)
        {
            Vector2 movementVector =
                new Vector2(velocity.x, velocity.y).normalized;

            anim.SetFloat("x", movementVector.x);
            anim.SetFloat("y", movementVector.y);
        }
    }

    public void checkforplayerdmg()
    {
        if (weaponTrigger == null)
            return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayers);
        filter.useLayerMask = true;
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];
        int hitCount = weaponTrigger.Overlap(filter, results);

        bool didHitSomething = false;

        for (int i = 0; i < hitCount; i++)
        {
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
            slowmoManager.TriggerSlowmo(0.5f, 0.2f); // Example: 0.5x speed for 0.2 seconds
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Detection Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Lose Target Range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);

        // Wander Area
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? spawnPosition : transform.position,
            wanderRadius
        );

        if (anim != null)
            anim.SetBool("isattacking", false);



        if (weaponTrigger != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(
                transform.position + (Vector3)weaponTrigger.offset,
                weaponTrigger.radius
            );
        }
    }
}