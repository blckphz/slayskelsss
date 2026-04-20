using UnityEngine;

public class TreeStumpRegrow : MonoBehaviour
{
    [Header("Settings")]
    public string treeID;
    public GameObject treePrefab;

    [Tooltip("Time in Game Days (e.g., 1.0 = one full day, 0.5 = 12 hours)")]
    public float regrowTime = 0.5f;

    private float cutTime;
    private DayNightCycle timeSystem;
    private bool initialized = false;

    void Start()
    {
        timeSystem = FindObjectOfType<DayNightCycle>();

        // If the stump was already in the scene on load, it needs its data back
        if (!initialized)
        {
            LoadData();
        }
    }

    void Update()
    {
        if (timeSystem == null || string.IsNullOrEmpty(treeID)) return;

        // Check if enough 'TotalTime' has passed since it was cut
        if (timeSystem.TotalTime >= (cutTime + regrowTime))
        {
            Regrow();
        }
    }

    public void Setup(string id, float time)
    {
        treeID = id;
        cutTime = time;
        initialized = true;
        SaveData();
    }

    private void Regrow()
    {
        // Spawn the full tree
        GameObject newTree = Instantiate(treePrefab, transform.position, Quaternion.identity);

        // Pass the ID to the new tree so it knows who it is
        treeItemBehav treeScript = newTree.GetComponent<treeItemBehav>();
        if (treeScript != null) treeScript.treeID = this.treeID;

        // Clean up PlayerPrefs for this ID
        PlayerPrefs.DeleteKey("Tree_" + treeID + "_isCut");
        PlayerPrefs.DeleteKey("Tree_" + treeID + "_cutTime");
        PlayerPrefs.Save();

        Destroy(gameObject);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("Tree_" + treeID + "_isCut", 1);
        PlayerPrefs.SetFloat("Tree_" + treeID + "_cutTime", cutTime);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey("Tree_" + treeID + "_cutTime"))
        {
            cutTime = PlayerPrefs.GetFloat("Tree_" + treeID + "_cutTime");
        }
    }
}