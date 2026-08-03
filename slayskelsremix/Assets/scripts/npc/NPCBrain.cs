using UnityEngine;
using Pathfinding;

public class NPCBrain : MonoBehaviour
{
    public enum NPCState { Idle, Gathering, FetchingFromChest, Refueling, Stowing, Wandering, Sleeping, MovingToHouse }

    [Header("Configuration")]
    public float interactDistance = 1.5f;
    public float actionCooldown = 1.0f;
    public float wanderRadius = 7f;

    [Header("State Debug")]
    [SerializeField] private NPCState currentState = NPCState.Idle;
    [SerializeField] private string jobDescription = "Idle";

    [Header("References")]
    public DayNightCycle dayNightCycle; // Drag your DayNightCycle manager here (or auto-find in Awake)
    public HouseOccuapyScript myHouse;   // Assigned home

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

        wanderTarget = new GameObject(gameObject.name + "_WanderTarget");

        if (dayNightCycle == null)
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
    }

    void Update()
    {
        float hour = (dayNightCycle != null) ? dayNightCycle.RawTime * 24f : 12f;
        bool isNightTime = hour >= 20f || hour < 8f; // 8 PM (20:00) to 8 AM (08:00)

        // Force wake up if it's past 8 AM
        if (!isNightTime && currentState == NPCState.Sleeping)
        {
            WakeUpFromSleep();
        }

        bool hasTarget = destinationSetter.target != null;
        float distanceToTarget = hasTarget ? Vector2.Distance(transform.position, destinationSetter.target.position) : float.MaxValue;
        bool isAtDestination = hasTarget && (distanceToTarget <= interactDistance || aiPath.reachedDestination);

        // State behavior updates
        switch (currentState)
        {
            case NPCState.Sleeping:
                // NPC stays here and does nothing until morning (8 AM)
                return;

            case NPCState.Wandering:
                wanderTimer += Time.deltaTime;
                if (isAtDestination || wanderTimer > 10f)
                {
                    FinishJob("Wander spot reached or timed out");
                }
                break;

            case NPCState.Gathering:
                if (!hasTarget) OnGatheringResourceDestroyed();
                break;
        }

        // Request job if idle or night time came up
        if ((destinationSetter.target == null || isNightTime) && Time.time >= nextActionTime && currentState != NPCState.Sleeping)
        {
            RequestNewJob(isNightTime);
        }

        // Perform Arrival Action
        if (isAtDestination)
        {
            HandleAction();
        }
    }

    private void RequestNewJob(bool isNightTime)
    {
        // 1. NIGHT TIME PRIORITY: Go Home & Sleep
        if (isNightTime)
        {
            if (myHouse == null)
            {
                myHouse = FindUnoccupiedHouse();
            }

            if (myHouse != null && myHouse.TryOccupy(gameObject))
            {
                StartJob(myHouse.sleepPoint, NPCState.MovingToHouse, "Heading home to sleep");
                return;
            }
        }

        // 2. DAY TIME JOBS (Fallback to Wandering if no tasks are available)
        if (currentState == NPCState.Idle)
        {
            StartWander();
        }
    }

    private void HandleAction()
    {
        if (Time.time < nextActionTime) return;

        Transform t = destinationSetter.target;
        if (t == null) return;

        switch (currentState)
        {
            case NPCState.MovingToHouse:
                // Reached the house -> Start Sleeping
                currentState = NPCState.Sleeping;
                jobDescription = "Sleeping";
                aiPath.canMove = false; // Freeze movement while sleeping
                break;

            case NPCState.Refueling:
                if (t.TryGetComponent(out CampfireBehav fire))
                {
                    fire.NPCInteract(inventory);
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
        }
    }

    private HouseOccuapyScript FindUnoccupiedHouse()
    {
        HouseOccuapyScript[] houses = FindObjectsByType<HouseOccuapyScript>(FindObjectsSortMode.None);

        HouseOccuapyScript nearest = null;
        float minDistance = float.MaxValue;

        foreach (var house in houses)
        {
            if (!house.IsOccupied || house.Owner == gameObject)
            {
                float dist = Vector2.Distance(transform.position, house.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = house;
                }
            }
        }

        return nearest;
    }

    private void WakeUpFromSleep()
    {
        aiPath.canMove = true;
        FinishJob("Woke up at 8 AM");
    }

    private void StartJob(Transform target, NPCState state, string desc)
    {
        destinationSetter.target = target;
        currentState = state;
        jobDescription = desc;
        aiPath.endReachedDistance = interactDistance - 0.2f;
        aiPath.canMove = true;
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
        aiPath.canMove = true;
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
        nextActionTime = 0;
        destinationSetter.target = null;
        currentState = NPCState.Idle;
        aiPath.canMove = true;
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