using UnityEngine;

public class treeItemBehav : ItemHealth
{
    [Header("Save State")]
    public bool isCut;
    public float cutTime;

    [Header("Prefabs")]
    public GameObject bottomPrefab;
    public GameObject fallingTopPrefab;

    public int lootAmount = 3;

    [Header("Stick Spawn")]
    public GameObject sticksPrefab;
    public Transform spawnPointA;
    public Transform spawnPointB;

    [Range(0f, 1f)]
    public float stickSpawnChance = 0.2f;

    private bool isDead = false;
    private Collider2D treeCollider;
    private DayNightCycle timeSystem;

    protected override void Awake()
    {
        base.Awake();
        treeCollider = GetComponent<Collider2D>();
        timeSystem = FindFirstObjectByType<DayNightCycle>();
    }

    // =========================
    // SPAWN INITIALIZATION
    // =========================

    public void OnSpawnFromWorld(bool hasSaveData)
    {
        EnsureID(hasSaveData);
        TrySpawnSticksOnSpawn();
    }

    // =========================
    // SAVE
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
        if (data == null)
        {
            EnsureID(false);
            return;
        }

        UniqOverworldItemID = data.treeID;

        isCut = data.isCut;
        cutTime = data.cutTime;
        transform.position = data.position;

        if (isCut)
            gameObject.SetActive(false);
    }

    // =========================
    // STICKS
    // =========================

    void TrySpawnSticksOnSpawn()
    {
        if (sticksPrefab == null || spawnPointA == null || spawnPointB == null)
            return;

        if (Random.value > stickSpawnChance)
            return;

        Transform chosen = (Random.value < 0.5f) ? spawnPointA : spawnPointB;

        Instantiate(sticksPrefab, chosen.position, Quaternion.identity);
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

        if (bottomPrefab != null)
        {
            GameObject stump = Instantiate(bottomPrefab, transform.position, Quaternion.identity);

            if (stump.TryGetComponent(out TreeStumpRegrow regrow))
                regrow.Setup(UniqOverworldItemID, cutTime);
        }

        if (fallingTopPrefab != null)
        {
            GameObject top = Instantiate(fallingTopPrefab, transform.position, transform.rotation);

            if (top.TryGetComponent(out treefallsequence fall))
                fall.SetupLoot(lootPrefab, lootAmount);
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (treeCollider != null)
            treeCollider.enabled = false;

        BuildingSaveManager.Instance?.SaveAfterChange();

        Destroy(gameObject);
    }
}