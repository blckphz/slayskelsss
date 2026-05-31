using UnityEngine;

public class TreeStumpRegrow : MonoBehaviour
{
    [Header("Settings")]
    public string treeID;

    public GameObject treePrefab;

    [Tooltip("Time in Game Days")]
    public float regrowTime = 0.5f;

    private float cutTime;

    private DayNightCycle timeSystem;

    private bool initialized = false;

    // ==========================================
    // UNITY
    // ==========================================

    void Start()
    {
        Debug.Log(
            $"[STUMP] Start() | treeID={treeID}");

        timeSystem =
            FindFirstObjectByType<DayNightCycle>();

        if (timeSystem == null)
        {
            Debug.LogError(
                "[STUMP] DayNightCycle NOT FOUND");
        }
        else
        {
            Debug.Log(
                $"[STUMP] DayNightCycle FOUND | CurrentTime={timeSystem.TotalTime}");
        }
    }

    void Update()
    {
        if (timeSystem == null)
            return;

        if (string.IsNullOrEmpty(treeID))
            return;

        float targetTime =
            cutTime + regrowTime;


        if (timeSystem.TotalTime >= targetTime)
        {
            Debug.Log(
                $"[STUMP] REGROW TRIGGERED | treeID={treeID}");

            Regrow();
        }
    }

    // ==========================================
    // SETUP
    // ==========================================

    public void Setup(string id, float time)
    {
        treeID = id;

        cutTime = time;

        initialized = true;

        Debug.Log(
            $"[STUMP] Setup() | treeID={treeID} | cutTime={cutTime}");
    }

    public void SetCutTime(float time)
    {
        cutTime = time;

        initialized = true;

        Debug.Log(
            $"[STUMP] SetCutTime() | cutTime={cutTime}");
    }

    public float GetCutTime()
    {
        Debug.Log(
            $"[STUMP] GetCutTime() | cutTime={cutTime}");

        return cutTime;
    }

    // ==========================================
    // REGROW
    // ==========================================

    private void Regrow()
    {
        Debug.Log(
            $"[STUMP] Regrow() START | treeID={treeID}");

        if (treePrefab == null)
        {
            Debug.LogError(
                "[STUMP] treePrefab is NULL");

            return;
        }

        GameObject newTree =
            Instantiate(
                treePrefab,
                transform.position,
                Quaternion.identity);

        Debug.Log(
            $"[STUMP] New tree instantiated | name={newTree.name}");

        treeItemBehav tree =
            newTree.GetComponent<treeItemBehav>();

        if (tree != null)
        {
            tree.UniqOverworldItemID = treeID;

            tree.isCut = false;

            tree.cutTime = 0f;

            Debug.Log(
                $"[STUMP] Tree state restored | ID={treeID}");
        }
        else
        {
            Debug.LogError(
                "[STUMP] treeItemBehav missing on prefab");
        }

        if (BuildingSaveManager.Instance != null)
        {
            Debug.Log(
                "[STUMP] Triggering autosave");

            BuildingSaveManager.Instance
                .SaveAfterChange();
        }
        else
        {
            Debug.LogError(
                "[STUMP] BuildingSaveManager.Instance NULL");
        }

        Debug.Log(
            $"[STUMP] Destroying stump | treeID={treeID}");

        Destroy(gameObject);
    }
}