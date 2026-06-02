using UnityEngine;

public class treeItemBehav : ItemHealth
{
    [Header("Save State")]
    public bool isCut;
    public float cutTime;

    [Header("Prefabs")]
    public GameObject bottomPrefab;
    public GameObject fallingTopPrefab;

    [Header("Loot")]
    public GameObject lootPrefab;
    public int lootAmount = 3;

    [Header("Stick Spawn (on world spawn)")]
    public GameObject sticksPrefab;
    public Transform spawnPointA;
    public Transform spawnPointB;

    [Header("Stick Spawn Chance (0 = 0%, 1 = 100%)")]
    [Range(0f, 1f)]
    public float stickSpawnChance = 0.2f;

    private bool isDead = false;

    private Collider2D treeCollider;
    private DayNightCycle timeSystem;

    protected override void Awake()
    {
        base.Awake();
        treeCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        timeSystem = FindFirstObjectByType<DayNightCycle>();
    }

    // =========================
    // SAVE / LOAD
    // =========================

    public TreeSaveData GetSaveData()
    {
        return new TreeSaveData
        {
            treeID = UniqOverworldItemID,
            isCut = isCut,
            cutTime = cutTime,
            position = transform.position
        };
    }

    public void LoadData(TreeSaveData data)
    {
        if (data == null) return;

        isCut = data.isCut;
        cutTime = data.cutTime;
        transform.position = data.position;

        if (isCut)
            gameObject.SetActive(false);
    }

    // =========================
    // WORLD SPAWN HOOK
    // =========================

    public void OnSpawnFromWorld()
    {
        Debug.Log("[Tree] Spawned from world -> rolling stick chance");
        TrySpawnSticksOnSpawn();
    }

    void TrySpawnSticksOnSpawn()
    {
        if (sticksPrefab == null || spawnPointA == null || spawnPointB == null)
        {
            Debug.LogWarning("[Tree] Missing sticksPrefab or spawn points");
            return;
        }

        float roll = Random.value;
        Debug.Log($"[Tree] Stick roll: {roll} vs chance {stickSpawnChance}");

        if (roll > stickSpawnChance)
        {
            Debug.Log("[Tree] No sticks spawned (failed chance roll)");
            return;
        }

        Transform chosenPoint =
            (Random.value < 0.5f) ? spawnPointA : spawnPointB;

        Debug.Log($"[Tree] Spawning sticks at {chosenPoint.name}");

        Instantiate(sticksPrefab, chosenPoint.position, Quaternion.identity);
    }

    // =========================
    // DEATH
    // =========================

    protected override void Die(ToolType killerTool)
    {
        if (isDead) return;
        isDead = true;

        isCut = true;

        cutTime = timeSystem != null
            ? timeSystem.TotalTime
            : Time.time;

        // stump
        if (bottomPrefab != null)
        {
            GameObject stump = Instantiate(
                bottomPrefab,
                transform.position,
                Quaternion.identity
            );

            if (stump.TryGetComponent(out TreeStumpRegrow regrow))
                regrow.Setup(UniqOverworldItemID, cutTime);
        }

        // falling top
        if (fallingTopPrefab != null)
        {
            GameObject top = Instantiate(
                fallingTopPrefab,
                transform.position,
                transform.rotation
            );

            if (top.TryGetComponent(out treefallsequence fall))
            {
                fall.SetupLoot(lootPrefab, lootAmount);
            }
        }

        // disable tree
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (treeCollider != null)
            treeCollider.enabled = false;

        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();

        Destroy(gameObject);
    }
}