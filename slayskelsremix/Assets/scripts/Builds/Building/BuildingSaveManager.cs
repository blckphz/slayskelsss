using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem; // Required for New Input System

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance;

    [System.Serializable]
    public class BuildingData
    {
        public int itemID;
        public string uniqueID;
        public Vector3 position;
        public int currentAmmo;
        public float plantProgress;
        public float fuelAmount;
        public bool isBurning;
        public int fuelItemID;
    }

    [System.Serializable]
    public class SaveData { public List<BuildingData> buildings = new List<BuildingData>(); }

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


    public void RegisterBuilding(GameObject obj)
    {
        if (obj != null && !placedBuildings.Contains(obj))
        {
            placedBuildings.Add(obj);
        }
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null && placedBuildings.Contains(obj))
        {
            placedBuildings.Remove(obj);
        }
    }

    public void SaveNow()
    {
        SaveData data = new SaveData();
        Debug.Log("<color=yellow>[SaveSystem] Starting Save Process...</color>");

        // Clean up the list before saving to remove any objects that were Destroyed but not Unregistered
        placedBuildings.RemoveAll(item => item == null);

        foreach (GameObject obj in placedBuildings)
        {
            BuildingData b = new BuildingData { position = obj.transform.position };

            if (obj.TryGetComponent(out ISaveableBuilding saveable))
            {
                b.itemID = saveable.GetItemID();
                saveable.GetSaveData(out b.currentAmmo, out b.plantProgress);
            }
            else if (obj.TryGetComponent(out BuildIdentity id) && id.item != null)
            {
                b.itemID = id.item.itemID;
            }
            else
            {
                Debug.LogWarning($"[Save] Skipping {obj.name}: No Interface or BuildIdentity found!");
                continue;
            }

            data.buildings.Add(b);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadBuildings()
    {
        if (!File.Exists(savePath)) { Debug.LogWarning("[Load] No save file found."); return; }
        if (database == null) { Debug.LogError("[Load] ItemDatabase is NOT assigned to BuildingSaveManager!"); return; }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"<color=orange>[LoadSystem] Found {data.buildings.Count} entries in JSON.</color>");

        foreach (var b in data.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null)
            {
                Debug.LogError($"[Load] ID {b.itemID} not found in ItemDatabase! Skipping.");
                continue;
            }

            GameObject prefab = null;
            if (item is buildSO bSO) prefab = bSO.placeablePrefab;
            else if (item is SeedSO sSO) prefab = sSO.plantPrefab;

            if (prefab == null)
            {
                Debug.LogError($"[Load] Item {item.name} has no associated Prefab!");
                continue;
            }

            GameObject obj = Instantiate(prefab, b.position, Quaternion.identity);
            obj.name = prefab.name;

            // Restore Interface data
            if (obj.TryGetComponent(out ISaveableBuilding s))
                s.LoadSaveData(b.currentAmmo, b.plantProgress);

            // Restore specific logic (Campfires/Turrets)
            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                campfire.fuelAmount = b.fuelAmount;
                campfire.isBurning = b.isBurning;
                if (b.fuelItemID != -1) campfire.fuelItem = database.GetItemByID(b.fuelItemID);
                campfire.SendMessage("UpdateVisuals", SendMessageOptions.DontRequireReceiver);
            }

            RegisterBuilding(obj);
            Debug.Log($"<color=orange>[LoadSystem] Successfully restored {obj.name} at {b.position}</color>");
        }
    }

    public void SaveAfterChange() => SaveNow();
}