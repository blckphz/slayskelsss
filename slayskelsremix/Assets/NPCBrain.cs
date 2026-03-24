using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(NpcAttack))]
public class NPCBrain : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 15f;
    public float searchInterval = 0.5f;
    public float attackStopDistance = 1.5f;

    [Header("Status")]
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
    }

    void Update()
    {
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            FindBestTarget();
            searchTimer = 0f;
        }

        HandleAction();
    }

    private void FindBestTarget()
    {
        // Priority 1: Enemies
        Transform enemy = GetClosest<enemyHealth>();
        if (enemy) { SetTarget(enemy, attackStopDistance); return; }

        // Priority 2: Ground Loot
        Transform loot = GetClosest<PickupItem>();
        if (loot) { SetTarget(loot, 0.1f); return; }

        // Priority 3: Resources
        Transform resource = GetClosest<ItemHealth>();
        if (resource) { SetTarget(resource, attackStopDistance); return; }

        destinationSetter.target = null;
    }

    private void SetTarget(Transform target, float dist)
    {
        destinationSetter.target = target;
        if (aiPath != null) aiPath.endReachedDistance = dist;
    }

    private void HandleAction()
    {
        if (destinationSetter.target == null) return;

        bool isAttackable = destinationSetter.target.GetComponent<IDamageable>() != null;

        if (isAttackable && targetInAttackTrigger)
        {
            attackModule.TryAttack(destinationSetter.target);
        }
        else
        {
            attackModule.StopAttacking();
        }
    }

    private Transform GetClosest<T>() where T : MonoBehaviour
    {
        T[] targets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        float closestDist = Mathf.Infinity;
        Transform best = null;

        foreach (T t in targets)
        {
            if (t == null || !t.gameObject.activeInHierarchy) continue;
            float d = Vector2.Distance(transform.position, t.transform.position);
            if (d < closestDist && d <= detectionRange)
            {
                closestDist = d;
                best = t.transform;
            }
        }
        return best;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (destinationSetter.target != null && other.transform == destinationSetter.target)
            targetInAttackTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (destinationSetter.target != null && other.transform == destinationSetter.target)
            targetInAttackTrigger = false;
    }
}