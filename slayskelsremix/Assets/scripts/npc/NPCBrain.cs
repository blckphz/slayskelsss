using UnityEngine;
using Pathfinding;

public class NPCBrain : MonoBehaviour
{
    public enum NPCState { Idle, Gathering, FetchingFromChest, Refueling, Stowing, Wandering }

    [Header("Configuration")]
    public float interactDistance = 1.5f; // Increased slightly for reliability
    public float actionCooldown = 1.0f;
    public float wanderRadius = 7f;
    [Range(0, 1)] public float maintenanceThreshold = 0.6f;

    [Header("State Debug")]
    [SerializeField] private NPCState currentState = NPCState.Idle;
    [SerializeField] private string jobDescription = "Idle";

    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private NpcInvBrain inventory;

    private float nextActionTime;
    private GameObject wanderTarget;
    private float wanderTimer;

    void Awake()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        inventory = GetComponent<NpcInvBrain>();

        // Create a unique helper object for wandering positions
        wanderTarget = new GameObject(gameObject.name + "_WanderTarget");
    }

    void Update()
    {
        // 1. Check if we have a target and how far away it is
        bool hasTarget = destinationSetter.target != null;
        float distanceToTarget = hasTarget ? Vector2.Distance(transform.position, destinationSetter.target.position) : float.MaxValue;

        // Reliability check: Are we close enough to act?
        bool isAtDestination = hasTarget && (distanceToTarget <= interactDistance || aiPath.reachedDestination);

        // 2. Handle State-Specific Logic
        switch (currentState)
        {
            case NPCState.Wandering:
                wanderTimer += Time.deltaTime;
                // If we reached the wander spot OR took too long (stuck), finish it.
                if (isAtDestination || wanderTimer > 10f)
                {
                    FinishJob("Wander spot reached or timed out");
                }
                break;

            case NPCState.Gathering:
                // If the tree was destroyed by someone else, reset.
                if (!hasTarget) OnGatheringResourceDestroyed();
                break;
        }

        // 3. Request New Job if idle or target-less
        if (destinationSetter.target == null && Time.time >= nextActionTime)
        {
            RequestNewJob();
        }

        // 4. Perform Action if we arrived
        if (isAtDestination)
        {
            HandleAction();
        }
    }

    private void RequestNewJob()
    {
        NPCState nextState;
        Transform nextTarget;
        string nextDesc;

    }

    private void StartJob(Transform target, NPCState state, string desc)
    {
        destinationSetter.target = target;
        currentState = state;
        jobDescription = desc;
        aiPath.endReachedDistance = interactDistance - 0.2f;
    }

    private void StartWander()
    {
        Vector3 randomPos = transform.position + (Vector3)(Random.insideUnitCircle * wanderRadius);
        wanderTarget.transform.position = randomPos;

        wanderTimer = 0;
        currentState = NPCState.Wandering;
        jobDescription = "Wandering Idle";
        destinationSetter.target = wanderTarget.transform;
        aiPath.endReachedDistance = 0.5f;
    }

    private void HandleAction()
    {
        if (Time.time < nextActionTime) return;

        Transform t = destinationSetter.target;
        if (t == null) return;

        switch (currentState)
        {
            case NPCState.Refueling:
                if (t.TryGetComponent(out CampfireBehav fire))
                {
                    fire.NPCInteract(inventory);
                    // CRITICAL: We call FinishJob immediately after interacting 
                    // so the NPC doesn't get stuck staring at the fire.
                    FinishJob("Refuel interaction completed");
                }
                break;

            case NPCState.Stowing:
                if (t.TryGetComponent(out ChestInventory chest))
                {
                    StowItems(chest);
                    FinishJob("Items stowed in chest");
                }
                break;

            case NPCState.FetchingFromChest:
                if (t.TryGetComponent(out ChestInventory c))
                {
                    WithdrawWood(c);
                    FinishJob("Wood fetched from chest");
                }
                break;

            case NPCState.Gathering:
                // Note: PickupItem logic is handled by triggers in the PickupItem script.
                // Here we handle attacking trees.
                break;
        }
    }

    public void FinishJob(string debugMsg)
    {
        destinationSetter.target = null;
        currentState = NPCState.Idle;
        nextActionTime = Time.time + actionCooldown;
    }

    private void OnGatheringResourceDestroyed()
    {
        destinationSetter.target = null;
        currentState = NPCState.Idle;
        nextActionTime = Time.time + 0.3f;
    }

    public void WakeUp()
    {
        // Used by PickupItem to tell the brain "You just picked something up, re-evaluate now!"
        nextActionTime = 0;
        destinationSetter.target = null;
        currentState = NPCState.Idle;
    }

    private void StowItems(ChestInventory chest)
    {
        for (int i = inventory.inventory.Count - 1; i >= 0; i--)
        {
            var slot = inventory.inventory[i];
            if (slot.item != null && chest.AddItem(slot.item, slot.count))
            {
                inventory.inventory.RemoveAt(i);
            }
        }
    }

    private void WithdrawWood(ChestInventory chest)
    {
        for (int i = 0; i < chest.chestItems.Count; i++)
        {
            var slot = chest.chestItems[i];
            if (slot.item != null)
            {
                int toTake = Mathf.Min(slot.count, 10);
                inventory.AddItem(slot.item, toTake);
                chest.RemoveItemAtIndex(i, toTake);
                return;
            }
        }
    }
}