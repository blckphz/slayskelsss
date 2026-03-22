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
    }

    [System.Serializable]
    public class SaveData
    {
        public List<BuildingData> buildings = new List<BuildingData>();
    }

    public ItemDatabase database;

    private List<GameObject> placedBuildings = new List<GameObject>();
    private Stack<GameObject> placementHistory = new Stack<GameObject>();

    private string savePath;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        savePath = Application.persistentDataPath + "/buildings.json";

        LoadBuildings();
    }

    // ---------------- REGISTER ----------------

    public void RegisterBuilding(GameObject obj)
    {
        placedBuildings.Add(obj);
        placementHistory.Push(obj);
    }

    // ---------------- SAVE ----------------

    public void SaveNow()
    {
        SaveData data = new SaveData();

        foreach (GameObject obj in placedBuildings)
        {
            if (obj == null) continue;

            BuildIdentity id = obj.GetComponent<BuildIdentity>();
            if (id == null || id.item == null) continue;

            data.buildings.Add(new BuildingData
            {
                itemID = id.item.itemID,
                position = obj.transform.position
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("[Save] Buildings saved.");
    }

    // ---------------- LOAD ----------------

    public void LoadBuildings()
    {
        if (!File.Exists(savePath))
            return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        foreach (var b in data.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null) continue;

            buildSO buildItem = item as buildSO;
            if (buildItem == null || buildItem.placeablePrefab == null) continue;

            GameObject obj = Instantiate(buildItem.placeablePrefab, b.position, Quaternion.identity);

            BuildIdentity id = obj.GetComponent<BuildIdentity>();
            if (id == null)
                id = obj.AddComponent<BuildIdentity>();

            id.item = buildItem;

            placedBuildings.Add(obj);
        }

    }

    // ---------------- UNDO ----------------

    public void UndoLastBuilding()
    {
        if (placementHistory.Count == 0)
        {
            Debug.Log("[Undo] Nothing to undo.");
            return;
        }

        GameObject last = placementHistory.Pop();

        if (last != null)
        {
            placedBuildings.Remove(last);
            Destroy(last);
        }

        SaveNow();

        Debug.Log("[Undo] Last building removed.");
    }
}