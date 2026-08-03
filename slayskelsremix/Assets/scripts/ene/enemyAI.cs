using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    private IAstarAI ai;
    private Animator anim;
    private Transform tr;

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

    private ContactFilter2D contactFilter;
    private readonly Collider2D[] overlapSingle = new Collider2D[1];
    private readonly Collider2D[] overlapMultiple = new Collider2D[10];

    private float detectionRangeSqr;
    private float loseTargetRangeSqr;
    private float attackRangeSqr;

    // Keeps last facing direction while idle
    private Vector2 lastDirection = Vector2.down;

    private static readonly int IsAttackingHash = Animator.StringToHash("isattacking");
    private static readonly int XHash = Animator.StringToHash("x");
    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Start()
    {
        tr = transform;
        ai = GetComponent<IAstarAI>();
        anim = GetComponent<Animator>();

        if (weaponTrigger == null)
            weaponTrigger = GetComponent<CircleCollider2D>();

        spawnPosition = tr.position;

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
        if (ai == null)
            return;

        if (currentTarget == null)
        {
            Wander();
            UpdateAnimator();
            return;
        }

        float distanceSqr = (currentTarget.position - tr.position).sqrMagnitude;

        if (!isChasing && distanceSqr <= detectionRangeSqr)
            isChasing = true;

        if (isChasing && distanceSqr > loseTargetRangeSqr)
        {
            isChasing = false;
            anim.SetBool(IsAttackingHash, false);
        }

        if (isChasing)
            ChaseAndAttack(distanceSqr);
        else
            Wander();

        UpdateAnimator();
    }

    private void ChaseAndAttack(float distanceSqr)
    {
        if (distanceSqr <= attackRangeSqr)
        {
            ai.isStopped = true;

            if (Time.time >= lastAttackTime + attackCooldown &&
                IsTargetInWeaponTrigger())
            {
                anim.SetBool(IsAttackingHash, true);
                lastAttackTime = Time.time;
            }
        }
        else
        {
            anim.SetBool(IsAttackingHash, false);

            ai.isStopped = false;
            ai.destination = currentTarget.position;
        }
    }

    private void Wander()
    {
        anim.SetBool(IsAttackingHash, false);

        if (Time.time < nextWanderTime)
            return;

        nextWanderTime = Time.time + wanderInterval;

        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;

        ai.isStopped = false;
        ai.destination = spawnPosition +
                         new Vector3(randomPoint.x, randomPoint.y, 0f);
    }

    private void StopMovement()
    {
        if (ai != null)
            ai.isStopped = true;

        if (anim != null)
        {
            anim.SetBool(IsAttackingHash, false);
            anim.SetFloat(SpeedHash, 0f);
        }
    }

    public void CancelAttack()
    {
        if (anim != null)
        {
            anim.SetBool(IsAttackingHash, false);
        }
    }

    private bool IsTargetInWeaponTrigger()
    {
        if (weaponTrigger == null)
            return false;

        return weaponTrigger.Overlap(contactFilter, overlapSingle) > 0;
    }

    private void UpdateAnimator()
    {
        if (ai == null || anim == null)
            return;

        Vector3 velocity = ai.velocity;
        float speed = velocity.magnitude;

        anim.SetFloat(SpeedHash, speed);

        if (speed > 0.05f)
        {
            lastDirection = new Vector2(velocity.x, velocity.y).normalized;

            Debug.Log(
                gameObject.name +
                " MOVING | Velocity: " + velocity +
                " | Last Direction: " + lastDirection
            );
        }
        else
        {
            Debug.Log(
                gameObject.name +
                " IDLE | Keeping Direction: " + lastDirection
            );
        }

        anim.SetFloat(XHash, lastDirection.x);
        anim.SetFloat(YHash, lastDirection.y);

    }

    public void checkforplayerdmg()
    {
        if (weaponTrigger == null)
            return;

        int hitCount = weaponTrigger.Overlap(contactFilter, overlapMultiple);

        bool didHitSomething = false;

        for (int i = 0; i < hitCount; i++)
        {
            healthMaster h = overlapMultiple[i].GetComponent<healthMaster>();

            if (h != null)
            {
                h.TakeDamage(damageAmount);
                didHitSomething = true;
            }

            overlapMultiple[i] = null;
        }

        anim.SetBool(IsAttackingHash, false);

        if (!didHitSomething)
        {
            slowmoManager.TriggerSlowmo(0.5f, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? spawnPosition : transform.position,
            wanderRadius
        );

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