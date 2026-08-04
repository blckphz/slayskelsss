using UnityEngine;

public class TreeStumpRegrow : ItemHealth
{
    [Header("Settings")]
    public string treeID;

    public GameObject treePrefab;


    [Header("Regrow Time (Game Days)")]
    public float regrowTime = 0.5f;


    public float cutTime;


    private DayNightCycle timeSystem;

    private bool alreadyRegrown = false;



    private void Start()
    {
        timeSystem =
            FindFirstObjectByType<DayNightCycle>();


        Debug.Log(
            "[TreeStumpRegrow] Loaded stump "
            + treeID
        );
    }



    private void Update()
    {
        if (alreadyRegrown)
            return;


        if (timeSystem == null)
            return;


        float elapsed =
            timeSystem.TotalTime - cutTime;



        if (elapsed >= regrowTime)
        {
            Regrow();
        }
    }





    public void Setup(string id, float time)
    {
        treeID = id;

        cutTime = time;


        Debug.Log(
            "[TreeStumpRegrow] Setup "
            + treeID +
            " cut at "
            + cutTime
        );
    }





    public float GetCutTime()
    {
        return cutTime;
    }





    private void Regrow()
    {
        if (treePrefab == null)
        {
            Debug.LogError(
                "[TreeStumpRegrow] Missing tree prefab!"
            );

            return;
        }


        alreadyRegrown = true;



        Debug.Log(
            "[TreeStumpRegrow] Regrowing tree "
            + treeID
        );



        GameObject newTree =
            Instantiate(
                treePrefab,
                transform.position,
                Quaternion.identity
            );



        if (newTree.TryGetComponent(out treeItemBehav tree))
        {
            tree.UniqOverworldItemID = treeID;

            tree.isCut = false;

            tree.cutTime = 0f;


            Debug.Log(
                "[TreeStumpRegrow] Tree restored "
                + treeID
            );
        }



        Destroy(gameObject);



        BuildingSaveManager.Instance?
            .SaveAfterChange();
    }
}