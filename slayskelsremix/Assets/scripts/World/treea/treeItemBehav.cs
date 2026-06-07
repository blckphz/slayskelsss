using UnityEngine;

public class treeItemBehav : ItemHealth
{
    [Header("Save State")]
    public bool isCut;
    public float cutTime;

    [Header("Prefabs")]
    public GameObject bottomPrefab;
    public GameObject fallingTopPrefab;


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
        TrySpawnSticksOnSpawn();
    }

    protected override void Start()
    {
        base.Start();

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
        TrySpawnSticksOnSpawn();

        if (isCut)
            gameObject.SetActive(false);
    }

    // =========================
    // WORLD SPAWN HOOK
    // =========================

    public void OnSpawnFromWorld()
    {
        TrySpawnSticksOnSpawn();
    }

    void TrySpawnSticksOnSpawn()
    {
        if (sticksPrefab == null || spawnPointA == null || spawnPointB == null)
        {
            return;
        }

        float roll = Random.value;

        if (roll > stickSpawnChance)
        {
            return;
        }

        Transform chosenPoint =
            (Random.value < 0.5f) ? spawnPointA : spawnPointB;


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
                fall.SetupLoot(lootPrefab, dropAmount);
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