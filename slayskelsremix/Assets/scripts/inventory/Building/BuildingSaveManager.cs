using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance;

    [System.Serializable]
    public class BuildingData
    {
        public int itemID;
        public Vector3 position;
        public float fuelAmount;
        public bool isBurning;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<BuildingData> buildings = new List<BuildingData>();
    }

    [Header("References")]
    public ItemDatabase database;

    [Header("Layers")]
    public string bigBuildablesLayerName = "bigbuildables";

    private List<GameObject> placedBuildings = new List<GameObject>();
    private string savePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/buildings.json";

        Debug.Log($"[SAVE] INIT → {savePath}");

        LoadBuildings();
        AutoRegisterSceneBuildings();
    }

    // ---------------- REGISTER ----------------

    public void RegisterBuilding(GameObject obj)
    {
        if (obj == null) return;

        if (!placedBuildings.Contains(obj))
        {
            placedBuildings.Add(obj);
            Debug.Log($"[SAVE] REGISTER → {obj.name}");
        }
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj == null) return;

        if (placedBuildings.Remove(obj))
        {
            Debug.Log($"[SAVE] UNREGISTER → {obj.name}");
        }
        else
        {
            Debug.LogWarning($"[SAVE] UNREGISTER FAILED (not found) → {obj.name}");
        }
    }

    // ---------------- SAVE ----------------

    public void SaveNow()
    {
        Debug.Log($"[SAVE] SAVE START → count {placedBuildings.Count}");

        SaveData data = new SaveData();

        foreach (GameObject obj in placedBuildings)
        {
            if (obj == null) continue;

            BuildIdentity id = obj.GetComponent<BuildIdentity>();
            if (id == null || id.item == null) continue;

            BuildingData b = new BuildingData
            {
                itemID = id.item.itemID,
                position = obj.transform.position
            };

            CampfireBehav campfire = obj.GetComponent<CampfireBehav>();
            if (campfire != null)
            {
                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;
            }

            data.buildings.Add(b);
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

        Debug.Log($"[SAVE] COMPLETE → saved {data.buildings.Count}");
    }

    public void SaveAfterChange()
    {
        Debug.Log("[SAVE] AUTO SAVE TRIGGERED");
        SaveNow();
    }

    // ---------------- LOAD ----------------

    public void LoadBuildings()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("[LOAD] no save file");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[LOAD] loading {data.buildings.Count}");

        foreach (var b in data.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null) continue;

            buildSO buildItem = item as buildSO;
            if (buildItem == null || buildItem.placeablePrefab == null) continue;

            GameObject obj = Instantiate(buildItem.placeablePrefab, b.position, Quaternion.identity);

            BuildIdentity id = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
            id.item = buildItem;

            RegisterBuilding(obj);
        }
    }

    // ---------------- AUTO REGISTER ----------------

    void AutoRegisterSceneBuildings()
    {
        GameObject[] all = FindObjectsOfType<GameObject>();
        int bigLayer = LayerMask.NameToLayer(bigBuildablesLayerName);

        int count = 0;

        foreach (var obj in all)
        {
            if (obj == null) continue;

            if (obj.GetComponent<BuildIdentity>() != null || obj.layer == bigLayer)
            {
                RegisterBuilding(obj);
                count++;
            }
        }

        Debug.Log($"[AUTO] registered {count} objects");
    }
}