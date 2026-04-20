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
        // Campfire specific data
        public float fuelAmount;
        public bool isBurning;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<BuildingData> buildings = new List<BuildingData>();
    }

    public ItemDatabase database;
    private List<GameObject> placedBuildings = new List<GameObject>();
    private string savePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/buildings.json";
        LoadBuildings();
    }

    public void RegisterBuilding(GameObject obj) => placedBuildings.Add(obj);
    public void UnregisterBuilding(GameObject obj) => placedBuildings.Remove(obj);

    public void SaveNow()
    {
        SaveData data = new SaveData();
        foreach (GameObject obj in placedBuildings)
        {
            if (obj == null) continue;

            BuildIdentity id = obj.GetComponent<BuildIdentity>();
            if (id == null || id.item == null) continue;

            BuildingData bData = new BuildingData
            {
                itemID = id.item.itemID,
                position = obj.transform.position
            };

            // If it's a campfire, grab its unique data
            CampfireBehav campfire = obj.GetComponent<CampfireBehav>();
            if (campfire != null)
            {
                bData.fuelAmount = campfire.fuelAmount;
                bData.isBurning = campfire.isBurning;
            }

            data.buildings.Add(bData);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

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

            BuildIdentity id = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
            id.item = buildItem;

            // Restore campfire state
            CampfireBehav campfire = obj.GetComponent<CampfireBehav>();
            if (campfire != null)
            {
                campfire.fuelAmount = b.fuelAmount;
                campfire.isBurning = b.isBurning;
                campfire.InitializeFromSave();
            }

            placedBuildings.Add(obj);
        }
    }
}