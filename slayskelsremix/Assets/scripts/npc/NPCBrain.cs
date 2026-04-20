using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(NpcAttack))]
[RequireComponent(typeof(NpcInvBrain))]
public class NPCBrain : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 20f;
    public float searchInterval = 0.5f;
    public float attackStopDistance = 1.5f;
    public float chestActionCooldown = 0.5f;

    [Header("Status")]
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
        // FAILSAFE 1: Immediate cleanup if the target object is destroyed/collected
        if (currentTargetID != -1 && destinationSetter.target == null)
        {
            ClearCurrentTarget();
        }

        // FAILSAFE 2: Stuck check (If target exists but we aren't moving)
        CheckIfStuck();

        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            FindBestTarget();
            searchTimer = 0f;
        }

        HandleAction();
    }

    private void CheckIfStuck()
    {
        if (destinationSetter.target != null && aiPath.velocity.sqrMagnitude < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 2f) // If stuck for 2 seconds
            {
                ClearCurrentTarget();
                stuckTimer = 0;
            }
        }
        else
        {
            stuckTimer = 0;
        }
    }

    private void HandleTargetDestroyed(int destroyedID)
    {
        if (currentTargetID == destroyedID)
        {
            ClearCurrentTarget();
            // Search again after a tiny delay to let the physics engine catch up
            Invoke(nameof(FindBestTarget), 0.05f);
        }
    }

    private void FindBestTarget()
    {
        // 1. ⚔️ ENEMIES
        Transform enemy = GetClosest<enemyHealth>();
        if (enemy) { SetTarget(enemy, attackStopDistance); return; }

        // 2. 📦 STORAGE (Priority if holding items)
        if (IsCarryingItems())
        {
            Transform chest = GetClosest<ChestInventory>();
            if (chest) { SetTarget(chest, 1.2f); return; }
        }

        // 3. ⛏ RESOURCES
        Transform resource = GetClosest<ItemHealth>();
        if (resource) { SetTarget(resource, attackStopDistance); return; }

        // 4. 💰 LOOT
        Transform loot = GetClosest<PickupItem>();
        if (loot) { SetTarget(loot, 0.1f); return; }

        // If we found nothing but still have a target, clear it
        if (destinationSetter.target != null) ClearCurrentTarget();
    }

    private void SetTarget(Transform target, float dist)
    {
        if (target == null)
        {
            ClearCurrentTarget();
            return;
        }

        if (destinationSetter.target != target)
        {
            destinationSetter.target = target;
            currentTargetID = target.gameObject.GetInstanceID();
        }

        if (aiPath != null) aiPath.endReachedDistance = dist;
    }

    private void ClearCurrentTarget()
    {
        if (NPCList.Instance != null) NPCList.Instance.ReleaseTarget(myID);
        destinationSetter.target = null;
        currentTargetID = -1;
        targetInAttackTrigger = false;
        stuckTimer = 0;
    }

    private void HandleAction()
    {
        // If the target is missing, stop everything
        if (destinationSetter.target == null)
        {
            attackModule.StopAttacking();
            targetInAttackTrigger = false;
            return;
        }

        if (targetInAttackTrigger)
        {
            var damageable = destinationSetter.target.GetComponent<IDamageable>();
            var chest = destinationSetter.target.GetComponent<ChestInventory>();

            if (damageable != null)
            {
                attackModule.TryAttack(destinationSetter.target);
            }
            else if (chest != null && IsCarryingItems())
            {
                if (Time.time >= nextChestActionTime)
                {
                    StoreItems(chest);
                    nextChestActionTime = Time.time + chestActionCooldown;
                }
            }
        }
        else
        {
            attackModule.StopAttacking();
        }
    }

    private bool IsCarryingItems() => inventory != null && inventory.inventory != null && inventory.inventory.Exists(s => s.count > 0);

    private void StoreItems(ChestInventory chest)
    {
        for (int i = inventory.inventory.Count - 1; i >= 0; i--)
        {
            var slot = inventory.inventory[i];
            if (slot.item != null && slot.count > 0)
            {
                if (chest.AddItem(slot.item, slot.count))
                    slot.count = 0;
            }
        }
        inventory.inventory.RemoveAll(s => s.count <= 0);
        ClearCurrentTarget(); // Go back to searching after storing
    }

    private Transform GetClosest<T>() where T : MonoBehaviour
    {
        T[] targets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        float closestDist = Mathf.Infinity;
        Transform best = null;

        foreach (T t in targets)
        {
            if (t == null || !t.gameObject.activeInHierarchy) continue;

            // Failsafe: Check if someone else claimed this
            if (NPCList.Instance != null && NPCList.Instance.IsTargetClaimed(t.transform, myID))
                continue;

            float d = Vector2.Distance(transform.position, t.transform.position);
            if (d < closestDist && d <= detectionRange)
            {
                closestDist = d;
                best = t.transform;
            }
        }

        if (best != null && NPCList.Instance != null)
        {
            NPCList.Instance.ReleaseTarget(myID);
            NPCList.Instance.ClaimTarget(best, myID);
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