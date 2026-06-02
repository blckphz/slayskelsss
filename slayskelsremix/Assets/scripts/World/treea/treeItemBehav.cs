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

        // disable original tree
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (treeCollider != null)
            treeCollider.enabled = false;

        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();

        Destroy(gameObject);
    }
}