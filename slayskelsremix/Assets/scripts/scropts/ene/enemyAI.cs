using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    private IAstarAI ai;
    private Animator anim;

    [Header("Targeting Settings")]
    private Transform currentTarget;
    public float detectionRefreshRate = 0.5f; // How often to search for the closest target

    [Header("Combat Settings")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public float damageAmount = 10f;
    private float lastAttackTime;

    [Header("Trigger Setup")]
    [SerializeField] private CircleCollider2D weaponTrigger;
    [SerializeField] private LayerMask targetLayers; // Set this to include both Player and Turret layers

    void Start()
    {
        ai = GetComponent<IAstarAI>();
        anim = GetComponent<Animator>();

        // Start the repeating search for the closest target
        InvokeRepeating(nameof(FindClosestTarget), 0f, detectionRefreshRate);

        if (weaponTrigger == null)
        {
            weaponTrigger = GetComponent<CircleCollider2D>();
        }
    }

    void Update()
    {
        if (currentTarget == null || ai == null)
        {
            ai.isStopped = true;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= attackRange)
        {
            ai.isStopped = true;
            TryAttack();
        }
        else
        {
            ai.isStopped = false;
            ai.destination = currentTarget.position;
        }

        UpdateAnimator();
    }

    void FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        // 1. Check for Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            closestDistance = dist;
            bestTarget = player.transform;
        }

        // 2. Check for Turrets
        TurretBehaviour[] turrets = FindObjectsOfType<TurretBehaviour>();
        foreach (TurretBehaviour turret in turrets)
        {
            // Only target turrets that are active/enabled
            if (!turret.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, turret.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = turret.transform;
            }
        }

        currentTarget = bestTarget;
    }

    void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // anim.SetTrigger("isAttacking"); 
            // Note: Ensure your animation calls checkforplayerdmg() via Animation Event
            lastAttackTime = Time.time;

            // If you don't use animations yet, you can call the damage check directly for testing:
            // checkforplayerdmg(); 
        }
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

        for (int i = 0; i < hitCount; i++)
        {
            // Attempt to damage Player
            playerHealth pHealth = results[i].GetComponent<playerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damageAmount);
                continue; // Move to next hit object
            }

            // Attempt to damage Turret
            TurretBehaviour tBehav = results[i].GetComponent<TurretBehaviour>();
            if (tBehav != null)
            {
                // Note: You need a TakeDamage method in TurretBehaviour
                tBehav.TakeDamage(damageAmount);
            }
        }
    }
}