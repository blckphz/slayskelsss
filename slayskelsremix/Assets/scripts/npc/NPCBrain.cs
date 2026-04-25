using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(NpcAttack))]
[RequireComponent(typeof(NpcInvBrain))]
public class NPCBrain : MonoBehaviour
{
    public float detectionRange = 20f;
    public float searchInterval = 0.5f;
    public float attackStopDistance = 1.5f;
    public float chestActionCooldown = 0.5f;

    [SerializeField] private bool targetInAttackTrigger = false;
    [SerializeField] private int currentTargetID = -1;

    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private NpcAttack attackModule;
    private NpcInvBrain inventory;

    private float searchTimer;
    private int myID;
    private float nextChestActionTime = 0f;
    private float stuckTimer = 0f;

    void Awake()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        attackModule = GetComponent<NpcAttack>();
        inventory = GetComponent<NpcInvBrain>();
        myID = gameObject.GetInstanceID();
    }

    void OnEnable() => NPCGlobalEvents.OnTargetDestroyed += HandleTargetDestroyed;
    void OnDisable() => NPCGlobalEvents.OnTargetDestroyed -= HandleTargetDestroyed;

    void Update()
    {
        Debug.Log($"[NPCBrain] Update | target={destinationSetter.target} trigger={targetInAttackTrigger}");

        if (currentTargetID != -1 && destinationSetter.target == null)
            Debug.Log("[NPCBrain] target missing");

        searchTimer += Time.deltaTime;

        if (searchTimer >= searchInterval)
        {
            Debug.Log("[NPCBrain] Searching...");
            FindBestTarget();
            searchTimer = 0f;
        }

        HandleAction();
    }

    private void HandleTargetDestroyed(int destroyedID)
    {
        Debug.Log($"[NPCBrain] Target destroyed: {destroyedID}");

        if (currentTargetID == destroyedID)
        {
            Debug.Log("[NPCBrain] clearing target");
            ClearCurrentTarget();
            Invoke(nameof(FindBestTarget), 0.05f);
        }
    }

    private void FindBestTarget()
    {
        Debug.Log("[NPCBrain] FindBestTarget");

        Transform enemy = GetClosest<enemyHealth>();
        if (enemy)
        {
            Debug.Log("[NPCBrain] enemy found");
            SetTarget(enemy, attackStopDistance);
            return;
        }

        Transform resource = GetClosest<ItemHealth>();
        if (resource)
        {
            Debug.Log("[NPCBrain] resource found");
            SetTarget(resource, attackStopDistance);
            return;
        }

        Transform loot = GetClosest<PickupItem>();
        if (loot)
        {
            Debug.Log("[NPCBrain] loot found");
            SetTarget(loot, 0.1f);
            return;
        }

        Debug.Log("[NPCBrain] nothing found");
    }

    private void SetTarget(Transform target, float dist)
    {
        if (target == null)
        {
            Debug.Log("[NPCBrain] SetTarget NULL");
            ClearCurrentTarget();
            return;
        }

        destinationSetter.target = target;
        currentTargetID = target.gameObject.GetInstanceID();

        Debug.Log($"[NPCBrain] target set -> {target.name}");
    }

    private void ClearCurrentTarget()
    {
        Debug.Log("[NPCBrain] ClearCurrentTarget");
        destinationSetter.target = null;
        currentTargetID = -1;
        targetInAttackTrigger = false;
    }

    private void HandleAction()
    {
        if (destinationSetter.target == null)
        {
            Debug.Log("[NPCBrain] NO TARGET -> stop");
            attackModule.StopAttacking();
            targetInAttackTrigger = false;
            return;
        }

        Debug.Log($"[NPCBrain] HandleAction trigger={targetInAttackTrigger}");

        if (targetInAttackTrigger)
        {
            Debug.Log("[NPCBrain] in range");

            var damageable = destinationSetter.target.GetComponent<IDamageable>();

            if (damageable != null)
            {
                Debug.Log("[NPCBrain] attacking");
                attackModule.TryAttack(destinationSetter.target);
            }
        }
        else
        {
            Debug.Log("[NPCBrain] out of range -> stop");
            attackModule.StopAttacking();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"[NPCBrain] TriggerStay {other.name}");

        if (destinationSetter.target != null && other.transform == destinationSetter.target)
        {
            Debug.Log("[NPCBrain] ENTER ATTACK RANGE");
            targetInAttackTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[NPCBrain] TriggerExit {other.name}");

        if (destinationSetter.target != null && other.transform == destinationSetter.target)
        {
            Debug.Log("[NPCBrain] EXIT ATTACK RANGE");
            targetInAttackTrigger = false;
        }
    }

    private Transform GetClosest<T>() where T : MonoBehaviour
    {
        T[] targets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);

        float closest = Mathf.Infinity;
        Transform best = null;

        foreach (var t in targets)
        {
            if (t == null) continue;

            float d = Vector2.Distance(transform.position, t.transform.position);

            if (d < closest)
            {
                closest = d;
                best = t.transform;
            }
        }

        return best;
    }
}