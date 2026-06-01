using UnityEngine;

public class TreeStumpRegrow : MonoBehaviour
{
    [Header("Settings")]
    public string treeID;

    public GameObject treePrefab;

    public float regrowTime = 0.5f;

    public float cutTime;

    private DayNightCycle timeSystem;

    private bool alreadyRegrown = false;

    void Start()
    {
        timeSystem =
            FindFirstObjectByType<DayNightCycle>();
    }

    void Update()
    {
        if (alreadyRegrown)
            return;

        if (timeSystem == null)
            return;

        if (timeSystem.TotalTime >=
            cutTime + regrowTime)
        {
            Regrow();
        }
    }

    public void Setup(string id, float time)
    {
        treeID = id;
        cutTime = time;
    }

    public float GetCutTime()
    {
        return cutTime;
    }

    private void Regrow()
    {
        alreadyRegrown = true;

        // =========================================
        // REMOVE OLD STUMP SAVE ENTRY
        // =========================================

        if (BuildingSaveManager.Instance != null)
        {
            var save =
                BuildingSaveManager.Instance
                .GetSaveData();

            if (save != null)
            {
                TreeSaveData oldData =
                    save.trees.Find(
                        t =>
                        t.treeID == treeID &&
                        t.isCut);

                if (oldData != null)
                {
                    save.trees.Remove(oldData);
                }
            }
        }

        // =========================================
        // SPAWN NEW TREE
        // =========================================

        GameObject newTree =
            Instantiate(
                treePrefab,
                transform.position,
                Quaternion.identity);

        if (newTree.TryGetComponent(
            out treeItemBehav tree))
        {
            tree.UniqOverworldItemID =
                treeID;

            tree.isCut = false;

            tree.cutTime = 0f;
        }

        // =========================================
        // SAVE NEW STATE
        // =========================================

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance
                .SaveAfterChange();
        }

        Destroy(gameObject);
    }
}