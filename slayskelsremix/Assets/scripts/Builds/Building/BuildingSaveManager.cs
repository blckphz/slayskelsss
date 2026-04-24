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
        public string uniqueID; // 🔥 Added to JSON data
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

        LoadBuildings();
        AutoRegisterSceneBuildings();
    }

    public void RegisterBuilding(GameObject obj)
    {
        if (obj != null && !placedBuildings.Contains(obj))
            placedBuildings.Add(obj);
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null) placedBuildings.Remove(obj);
    }

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

            // 🔥 Save the Unique ID
            tableBehav table = obj.GetComponent<tableBehav>();
            if (table != null) b.uniqueID = table.ID;

            // Campfire specific data
            CampfireBehav campfire = obj.GetComponent<CampfireBehav>();
            if (campfire != null)
            {
                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;
            }

            data.buildings.Add(b);
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("[SAVE] Data saved with unique IDs.");
    }

    public void SaveAfterChange() => SaveNow();

    public void LoadBuildings()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        foreach (var b in data.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null) continue;

            buildSO buildItem = item as buildSO;
            if (buildItem == null || buildItem.placeablePrefab == null) continue;

            GameObject obj = Instantiate(buildItem.placeablePrefab, b.position, Quaternion.identity);

            // 🔥 Restore the Unique ID
            tableBehav table = obj.GetComponent<tableBehav>();
            if (table != null && !string.IsNullOrEmpty(b.uniqueID))
            {
                table.LoadPersistentID(b.uniqueID);
            }

            BuildIdentity id = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
            id.item = buildItem;

            RegisterBuilding(obj);
        }
    }

    void AutoRegisterSceneBuildings()
    {
        int bigLayer = LayerMask.NameToLayer(bigBuildablesLayerName);
        foreach (var id in FindObjectsOfType<BuildIdentity>())
            RegisterBuilding(id.gameObject);
    }
}