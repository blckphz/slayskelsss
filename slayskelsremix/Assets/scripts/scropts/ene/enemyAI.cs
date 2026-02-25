using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    private IAstarAI ai;
    private Animator anim;
    private Transform playerTransform;

    [Header("Combat Settings")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public float damageAmount = 10f;
    private float lastAttackTime;

    [Header("Trigger Setup")]
    [SerializeField] private CircleCollider2D weaponTrigger;
    [SerializeField] private LayerMask playerLayer;

    void Start()
    {
        ai = GetComponent<IAstarAI>();
        anim = GetComponent<Animator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("EnemyAI: No object with tag 'Player' found in scene!");
        }

        if (weaponTrigger == null)
        {
            weaponTrigger = GetComponent<CircleCollider2D>();
            if (weaponTrigger == null) Debug.LogError("EnemyAI: No CircleCollider2D found on this object!");
        }
    }

    void Update()
    {
        if (playerTransform == null || ai == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            ai.isStopped = true;
            TryAttack();
        }
        else
        {
            ai.isStopped = false;
            ai.destination = playerTransform.position;
        }

        UpdateAnimator();
    }

    void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            //anim.SetTrigger("isAttacking");
            lastAttackTime = Time.time;
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
        //anim.SetFloat("Speed", ai.isStopped ? 0f : velocity.magnitude);
    }

    // CALLED BY ANIMATION EVENT
    public void checkforplayerdmg()
    {

        if (weaponTrigger == null)
        {
            Debug.LogError("EnemyAI: WeaponTrigger is null! Damage check aborted.");
            return;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayer);
        filter.useLayerMask = true;
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[5];
        int hitCount = weaponTrigger.Overlap(filter, results);


        for (int i = 0; i < hitCount; i++)
        {
            playerHealth pHealth = results[i].GetComponent<playerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogWarning("EnemyAI: Hit " + results[i].name + " but it doesn't have a playerHealth script!");
            }
        }
    }
}