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
        public int currentAmmo;
        public float plantProgress;
        public float fuelAmount;
        public bool isBurning;
        public int fuelItemID;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<BuildingData> buildings = new List<BuildingData>();
        public List<NpcInventorySaveData> npcInventories = new List<NpcInventorySaveData>();
        public List<BushSaveData> bushes = new List<BushSaveData>(); // New Bush List
    }

    public ItemDatabase database;
    private List<GameObject> placedBuildings = new List<GameObject>();
    private string savePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/buildings.json";
        LoadEverything();
    }

    public void RegisterBuilding(GameObject obj)
    {
        if (obj != null && !placedBuildings.Contains(obj))
            placedBuildings.Add(obj);
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null && placedBuildings.Contains(obj))
            placedBuildings.Remove(obj);
    }

    public void SaveNow()
    {
        SaveData data = new SaveData();

        // 1. SAVE BUILDINGS
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

            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;
                b.fuelItemID = campfire.fuelItem != null ? campfire.fuelItem.itemID : -1;
            }
            data.buildings.Add(b);
        }

        // 2. SAVE NPCs
        NpcInvBrain[] npcs = FindObjectsOfType<NpcInvBrain>();
        foreach (NpcInvBrain npc in npcs)
        {
            data.npcInventories.Add(npc.GetSaveData());
        }

        // 3. SAVE BUSHES
        BushBehav[] sceneBushes = FindObjectsOfType<BushBehav>();
        foreach (BushBehav bush in sceneBushes)
        {
            data.bushes.Add(new BushSaveData
            {
                bushID = bush.bushID,
                isHarvested = bush.IsHarvested,
                health = bush.health,
                timeAtHarvest = bush.GetSaveData().timeAtHarvest 
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadEverything()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // LOAD BUILDINGS
        foreach (var b in data.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null) continue;

            GameObject prefab = (item is buildSO bSO) ? bSO.placeablePrefab : (item is SeedSO sSO) ? sSO.plantPrefab : null;
            if (prefab == null) continue;

            GameObject obj = Instantiate(prefab, b.position, Quaternion.identity);
            obj.name = prefab.name;

            if (obj.TryGetComponent(out ISaveableBuilding saveable))
                saveable.LoadSaveData(b.currentAmmo, b.plantProgress);

            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                campfire.fuelAmount = b.fuelAmount;
                campfire.isBurning = b.isBurning;
                if (b.fuelItemID != -1) campfire.fuelItem = database.GetItemByID(b.fuelItemID);
                campfire.SendMessage("UpdateVisuals", SendMessageOptions.DontRequireReceiver);
            }
            RegisterBuilding(obj);
        }

        // LOAD NPCs
        NpcInvBrain[] sceneNpcs = FindObjectsOfType<NpcInvBrain>();
        foreach (NpcInvBrain npc in sceneNpcs)
        {
            var npcSave = data.npcInventories.Find(x => x.npcId == npc.npcId);
            if (npcSave != null) npc.LoadFromSave(npcSave, database);
        }

        // LOAD BUSHES
        BushBehav[] sceneBushes = FindObjectsOfType<BushBehav>();
        foreach (BushBehav bush in sceneBushes)
        {
            var bushSave = data.bushes.Find(x => x.bushID == bush.bushID);
            if (bushSave != null) bush.LoadData(bushSave);
        }
    }

    public void SaveAfterChange()
    {
        // Calling the main save logic
        SaveNow();
    }

}