using Pathfinding;
using UnityEngine;

public class NPCJobManager : MonoBehaviour
{
    public enum JobState
    {
        Idle,
        SearchingTree,
        MovingToTree,
        ChoppingTree
    }

    [Header("Settings")]
    [SerializeField] private float chopRange = 1.6f;
    [SerializeField] private float treeSearchInterval = 3f;
    [SerializeField] private bool showDebug = true;

    private JobState currentState = JobState.Idle;

    private treeItemBehav currentTree;

    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;
    private NpcInvBrain inventory;

    private float nextTreeSearchTime;

    private void Awake()
    {
        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        inventory = GetComponent<NpcInvBrain>();
    }

    private void Start()
    {
        currentState = JobState.SearchingTree;
    }

    private void Update()
    {
        switch (currentState)
        {
            case JobState.SearchingTree:
                HandleTreeSearch();
                break;

            case JobState.MovingToTree:
                HandleMoveToTree();
                break;

            case JobState.ChoppingTree:
                HandleChopping();
                break;
        }
    }

    // =====================================================
    // SEARCH
    // =====================================================

    private void HandleTreeSearch()
    {
        if (Time.time < nextTreeSearchTime)
            return;

        nextTreeSearchTime = Time.time + treeSearchInterval;

        currentTree = FindClosestTree();

        if (currentTree == null)
            return;

        destinationSetter.target = currentTree.transform;
        aiPath.canMove = true;

        currentState = JobState.MovingToTree;
    }

    // =====================================================
    // MOVE
    // =====================================================

    private void HandleMoveToTree()
    {
        if (currentTree == null)
        {
            currentState = JobState.SearchingTree;
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTree.transform.position);

        if (distance <= chopRange)
        {
            aiPath.canMove = false;
            currentState = JobState.ChoppingTree;
        }
    }

    // =====================================================
    // CHOP (GENERIC ITEM USAGE)
    // =====================================================

    private void HandleChopping()
    {
        if (currentTree == null)
        {
            currentState = JobState.SearchingTree;
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTree.transform.position);

        if (distance > chopRange)
        {
            aiPath.canMove = true;
            currentState = JobState.MovingToTree;
            return;
        }

        // ✅ GENERIC TOOL SEARCH (NO HARDCODE)
        UseableItem tool = inventory.GetBestUsableItem(u =>
            u.abilityToExecute is ToolsSO t &&
            t.toolType == ToolType.Axe);

        if (tool == null)
        {
            Debug.Log($"{name}: No axe found.");
            return;
        }

        inventory.TryUseItem(tool, transform, currentTree.transform, currentTree.gameObject);
    }

    // =====================================================
    // TREE FINDER
    // =====================================================

    private treeItemBehav FindClosestTree()
    {
        var trees = FindObjectsByType<treeItemBehav>(FindObjectsSortMode.None);

        float bestDist = float.MaxValue;
        treeItemBehav best = null;

        foreach (var t in trees)
        {
            float d = Vector2.Distance(transform.position, t.transform.position);

            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        return best;
    }
}