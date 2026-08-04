using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(enemyWalkAI))]
public class EnemyAI : MonoBehaviour
{
    private Animator anim;
    private enemyWalkAI movement;

    [Header("Target")]
    [SerializeField] private Transform currentTarget;

    [Header("Behavior")]
    public bool passiveEnemy = false;
    private bool hasBeenAttacked;
    private bool isChasing;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float loseTargetRange = 7f;

    [Header("Combat")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public int damageAmount = 10;

    private float lastAttackTime;

    [Header("Weapon Trigger")]
    [SerializeField] private CircleCollider2D weaponTrigger;
    [SerializeField] private LayerMask targetLayers;

    private ContactFilter2D contactFilter;

    private readonly Collider2D[] overlapSingle = new Collider2D[1];
    private readonly Collider2D[] overlapMultiple = new Collider2D[10];

    private float detectionRangeSqr;
    private float loseTargetRangeSqr;
    private float attackRangeSqr;

    private static readonly int IsAttackingHash = Animator.StringToHash("isattacking");

    private void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<enemyWalkAI>();

        if (movement == null)
        {
            Debug.LogError($"[EnemyAI] Missing 'enemyWalkAI' component on {gameObject.name}!", this);
        }

        if (weaponTrigger == null)
            weaponTrigger = GetComponent<CircleCollider2D>();

        // Cache squared distances for performant distance checks
        detectionRangeSqr = detectionRange * detectionRange;
        loseTargetRangeSqr = loseTargetRange * loseTargetRange;
        attackRangeSqr = attackRange * attackRange;

        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = true
        };
        contactFilter.SetLayerMask(targetLayers);

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
        if (currentTarget == null)
        {
            FindPlayer(); // Re-check if player was instantiated late
            if (currentTarget == null) return;
        }

        float distanceSqr = (currentTarget.position - transform.position).sqrMagnitude;

        // Evaluate Chasing State
        if (!isChasing)
        {
            if (passiveEnemy)
            {
                if (hasBeenAttacked) isChasing = true;
            }
            else
            {
                if (distanceSqr <= detectionRangeSqr) isChasing = true;
            }
        }

        // Stop Chasing if target gets too far
        if (isChasing && distanceSqr > loseTargetRangeSqr)
        {
            isChasing = false;
            anim.SetBool(IsAttackingHash, false);
        }

        // Behavior Execution
        if (isChasing)
        {
            Chase(distanceSqr);
        }
        else if (movement != null)
        {
            movement.Wander(); // Perform idle wandering when not chasing
        }

        // Drive Blend Tree Parameters
        if (movement != null)
        {
            movement.UpdateMovementAnimation();
        }
    }

    private void Chase(float distanceSqr)
    {
        if (distanceSqr <= attackRangeSqr)
        {
            movement.StopMovement();

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                if (IsTargetInWeaponTrigger())
                {
                    anim.SetBool(IsAttackingHash, true);
                    lastAttackTime = Time.time;
                }
            }
        }
        else
        {
            anim.SetBool(IsAttackingHash, false);
            movement.MoveTo(currentTarget.position);
        }
    }

    public void BecomeAggressive()
    {
        hasBeenAttacked = true;
        isChasing = true;
    }

    public void CancelAttack()
    {
        anim.SetBool(IsAttackingHash, false);
    }

    private bool IsTargetInWeaponTrigger()
    {
        if (weaponTrigger == null) return false;

        return weaponTrigger.Overlap(contactFilter, overlapSingle) > 0;
    }

    // Called via Animation Event mid-swing
    public void checkforplayerdmg()
    {
        if (weaponTrigger == null) return;

        int hitCount = weaponTrigger.Overlap(contactFilter, overlapMultiple);
        bool hitSomething = false;

        for (int i = 0; i < hitCount; i++)
        {
            if (overlapMultiple[i] != null)
            {
                healthMaster h = overlapMultiple[i].GetComponent<healthMaster>();
                if (h != null)
                {
                    h.TakeDamage(damageAmount);
                    hitSomething = true;
                }

                overlapMultiple[i] = null; // Clean array slot
            }
        }

        // Slowmo feedback if attack missed
        if (!hitSomething && hitCount == 0)
        {
            slowmoManager.TriggerSlowmo(0.5f, 0.2f);
        }
    }
}