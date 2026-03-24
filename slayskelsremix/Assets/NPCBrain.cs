using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(NpcAttack))]
public class NPCBrain : MonoBehaviour
{
    [Header("Detection (Finding Target)")]
    public float detectionRange = 20f;
    public float searchInterval = 0.5f;

    [Header("State")]
    [SerializeField] private bool targetInAttackTrigger = false;

    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private NpcAttack attackModule;
    private float searchTimer;

    void Awake()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        attackModule = GetComponent<NpcAttack>();

        // Set A* stopping distance to be very close so they stay in trigger
        if (aiPath != null) aiPath.endReachedDistance = 0.5f;
    }

    void Update()
    {
        searchTimer += Time.deltaTime;

        // 1. Find the target from a distance
        if (searchTimer >= searchInterval)
        {
            FindClosestEnemy();
            searchTimer = 0f;
        }

        // 2. Attack logic based strictly on the Trigger status
        if (targetInAttackTrigger && destinationSetter.target != null)
        {
            attackModule.TryAttack(destinationSetter.target);
        }
        else
        {
            attackModule.StopAttacking();
        }
    }

    private void FindClosestEnemy()
    {
        enemyHealth[] allEnemies = Object.FindObjectsByType<enemyHealth>(FindObjectsSortMode.None);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (enemyHealth enemy in allEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= detectionRange)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        if (destinationSetter.target != closestEnemy)
            destinationSetter.target = closestEnemy;
    }

    // --- TRIGGER LOGIC ---
    // This is what actually allows the NPC to swing
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (destinationSetter.target != null && collision.transform == destinationSetter.target)
        {
            targetInAttackTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (destinationSetter.target != null && collision.transform == destinationSetter.target)
        {
            targetInAttackTrigger = false;
        }
    }
}