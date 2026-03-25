using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(NpcAttack))]
[RequireComponent(typeof(NpcInvBrain))]
public class NPCBrain : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 15f;
    public float searchInterval = 0.5f;
    public float attackStopDistance = 1.5f;

    [Header("Storage")]
    public int storeThreshold = 5;

    [Header("Status")]
    [SerializeField] private bool targetInAttackTrigger = false;

    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private NpcAttack attackModule;
    private NpcInvBrain inventory;
    private float searchTimer;

    private float nextChestActionTime = 0f;
    public float chestActionCooldown = 0.5f;

    void Awake()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        attackModule = GetComponent<NpcAttack>();
        inventory = GetComponent<NpcInvBrain>();
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

    // ================= TARGETING =================

    private void FindBestTarget()
    {
        // ⚔️ 1. ENEMIES (ALWAYS PRIORITY)
        Transform enemy = GetClosest<enemyHealth>();
        if (enemy) { SetTarget(enemy, attackStopDistance); return; }

        // 📦 2. IF INVENTORY FULL → GO STORE
        if (ShouldStoreItems())
        {
            Transform chest = GetClosest<ChestInventory>();
            if (chest) { SetTarget(chest, 1.2f); return; }
        }

        // ⛏ 3. RESOURCES
        Transform resource = GetClosest<ItemHealth>();
        if (resource) { SetTarget(resource, attackStopDistance); return; }

        // 🧰 4. CHESTS (only used for storing)
        Transform chestFallback = GetClosest<ChestInventory>();
        if (chestFallback) { SetTarget(chestFallback, 1.2f); return; }

        // 💰 5. GROUND LOOT
        Transform loot = GetClosest<PickupItem>();
        if (loot) { SetTarget(loot, 0.1f); return; }

        destinationSetter.target = null;
    }

    private void SetTarget(Transform target, float dist)
    {
        destinationSetter.target = target;
        if (aiPath != null) aiPath.endReachedDistance = dist;
    }

    // ================= ACTION =================

    private void HandleAction()
    {
        if (destinationSetter.target == null) return;

        var damageable = destinationSetter.target.GetComponent<IDamageable>();
        var chest = destinationSetter.target.GetComponent<ChestInventory>();

        // ⚔️ ATTACK (UNCHANGED)
        if (damageable != null && targetInAttackTrigger)
        {
            attackModule.TryAttack(destinationSetter.target);
            return;
        }

        // 🧰 CHEST STORAGE ONLY
        if (chest != null && targetInAttackTrigger)
        {
            if (ShouldStoreItems())
            {
                if (Time.time >= nextChestActionTime)
                {
                    StoreItems(chest);
                    nextChestActionTime = Time.time + chestActionCooldown;
                }
            }

            return;
        }

        attackModule.StopAttacking();
    }

    // ================= STORAGE =================

    private bool ShouldStoreItems()
    {
        if (inventory == null) return false;

        int total = 0;
        foreach (var slot in inventory.inventory)
        {
            if (slot.item != null)
                total += slot.count;
        }

        return total >= storeThreshold;
    }

    private void StoreItems(ChestInventory chest)
    {
        if (inventory == null || chest == null) return;

        foreach (var slot in inventory.inventory)
        {
            if (slot.item != null && slot.count > 0)
            {
                bool added = chest.AddItem(slot.item, slot.count);
                if (added)
                {
                    slot.count = 0;
                }
            }
        }

        // Clean empty slots
        inventory.inventory.RemoveAll(s => s.count <= 0);
    }

    // ================= HELPERS =================

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