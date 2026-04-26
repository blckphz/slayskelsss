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
        public string uniqueID;
        public Vector3 position;

        // 🔥 Campfire data
        public float fuelAmount;
        public bool isBurning;
        public int fuelItemID;
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

        LoadBuildings();
        AutoRegisterSceneBuildings();
    }

    // =========================
    // REGISTER
    // =========================

    public void RegisterBuilding(GameObject obj)
    {
        if (obj != null && !placedBuildings.Contains(obj))
            placedBuildings.Add(obj);
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null)
            placedBuildings.Remove(obj);
    }

    // =========================
    // SAVE
    // =========================

    public void SaveNow()
    {
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

            // 🔥 Unique ID
            tableBehav table = obj.GetComponent<tableBehav>();
            if (table != null)
                b.uniqueID = table.ID;

            // 🔥 Campfire SAVE
            CampfireBehav campfire = obj.GetComponent<CampfireBehav>();
            if (campfire != null)
            {
                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;
                b.fuelItemID = campfire.fuelItem != null ? campfire.fuelItem.itemID : -1;

                Debug.Log($"<color=cyan>[SAVE]</color> Campfire [{b.uniqueID}] Fuel:{b.fuelAmount} Burning:{b.isBurning}");
            }

            data.buildings.Add(b);
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log($"<color=cyan>[SAVE]</color> Saved {data.buildings.Count} buildings.");
    }

    public void SaveAfterChange() => SaveNow();

    // =========================
    // LOAD
    // =========================

    public void LoadBuildings()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("<color=yellow>[LOAD]</color> No save file found.");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        foreach (var b in data.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null) continue;

            buildSO buildItem = item as buildSO;
            if (buildItem == null || buildItem.placeablePrefab == null) continue;

            GameObject obj = Instantiate(buildItem.placeablePrefab, b.position, Quaternion.identity);

            // 🔥 Restore Unique ID
            tableBehav table = obj.GetComponent<tableBehav>();
            if (table != null && !string.IsNullOrEmpty(b.uniqueID))
            {
                table.LoadPersistentID(b.uniqueID);
            }

            BuildIdentity id = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
            id.item = buildItem;

            // 🔥 LOAD CAMPFIRE DATA
            CampfireBehav campfire = obj.GetComponent<CampfireBehav>();
            if (campfire != null)
            {
                campfire.fuelAmount = b.fuelAmount;
                campfire.isBurning = b.isBurning;

                if (b.fuelItemID != -1)
                    campfire.fuelItem = database.GetItemByID(b.fuelItemID);


                // IMPORTANT → update visuals AFTER loading
                campfire.SendMessage("UpdateVisuals", SendMessageOptions.DontRequireReceiver);
            }

            RegisterBuilding(obj);
        }

        Debug.Log($"<color=green>[LOAD]</color> Loaded {data.buildings.Count} buildings.");
    }

    // =========================
    // AUTO REGISTER
    // =========================

    void AutoRegisterSceneBuildings()
    {
        foreach (var id in FindObjectsOfType<BuildIdentity>())
            RegisterBuilding(id.gameObject);
    }
}