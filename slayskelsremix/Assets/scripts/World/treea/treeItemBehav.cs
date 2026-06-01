using System.Collections;
using UnityEngine;

public class treeItemBehav : ItemHealth
{
    [Header("Save State")]
    public bool isCut;
    public float cutTime;

    private bool isDead = false;
    private Collider2D treeCollider;
    private DayNightCycle timeSystem;
    public GameObject bottomPrefab;


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

        // IMPORTANT FIX:
        // DO NOT spawn stump here anymore
        if (isCut)
        {
            gameObject.SetActive(false);
        }
    }

    protected override void Die(ToolType killerTool)
    {
        if (isDead) return;

        isDead = true;

        isCut = true;
        cutTime = timeSystem != null ? timeSystem.TotalTime : Time.time;

        if (bottomPrefab != null)
        {
            GameObject stump =
                Instantiate(bottomPrefab, transform.position, Quaternion.identity);

            if (stump.TryGetComponent(out TreeStumpRegrow regrow))
                regrow.Setup(UniqOverworldItemID, cutTime);
        }

        StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (treeCollider != null) treeCollider.enabled = false;

        yield return new WaitForSeconds(0.05f);

        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();

        Destroy(gameObject);
    }
}