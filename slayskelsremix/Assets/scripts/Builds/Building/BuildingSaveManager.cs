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
        public int structuralDurability;

        public float fuelAmount;
        public bool isBurning;
        public int fuelItemID;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<BuildingData> buildings = new List<BuildingData>();
        public List<NpcInventorySaveData> npcInventories = new List<NpcInventorySaveData>();
        public List<BushSaveData> bushes = new List<BushSaveData>();
        public List<TreeSaveData> trees = new List<TreeSaveData>();
        public List<SoilData> soilTiles = new List<SoilData>();
    }

    public ItemDatabase database;

    [Header("TREE PREFABS")]
    public GameObject treePrefab;
    public GameObject stumpPrefab;

    private List<GameObject> placedBuildings = new List<GameObject>();
    private string savePath;
    private SaveData cachedData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/buildings.json";
    }

    private void Start()
    {
        LoadEverything();
    }

    // =====================================================
    // REGISTER
    // =====================================================

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

    // =====================================================
    // SAVE
    // =====================================================

    public void SaveNow()
    {
        SaveData data = new SaveData();

        placedBuildings.RemoveAll(x => x == null);

        foreach (GameObject obj in placedBuildings)
        {
            BuildingData b = new BuildingData();
            b.position = obj.transform.position;

            if (obj.TryGetComponent(out ISaveableBuilding saveable))
            {
                b.itemID = saveable.GetItemID();
                saveable.GetSaveData(out b.currentAmmo, out b.plantProgress, out b.structuralDurability);
            }
            else if (obj.TryGetComponent(out BuildIdentity id))
            {
                if (id.item != null)
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

        // NPCS
        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
            data.npcInventories.Add(npc.GetSaveData());

        // BUSHES
        foreach (BushBehav bush in FindObjectsByType<BushBehav>(FindObjectsSortMode.None))
        {
            data.bushes.Add(new BushSaveData
            {
                bushID = bush.UniqOverworldItemID,
                isHarvested = bush.IsHarvested,
                health = bush.health,
                timeAtHarvest = bush.GetSaveData().timeAtHarvest
            });
        }

        // TREES + STUMPS (ONLY DATA, NO SPAWNING HERE)
        foreach (treeItemBehav tree in FindObjectsByType<treeItemBehav>(FindObjectsSortMode.None))
            data.trees.Add(tree.GetSaveData());

        foreach (TreeStumpRegrow stump in FindObjectsByType<TreeStumpRegrow>(FindObjectsSortMode.None))
        {
            data.trees.Add(new TreeSaveData
            {
                treeID = stump.treeID,
                isCut = true,
                cutTime = stump.GetCutTime(),
                position = stump.transform.position
            });
        }

        TiltManager tilt = FindFirstObjectByType<TiltManager>();
        if (tilt != null)
            data.soilTiles = tilt.GetSaveData();

        cachedData = data;

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    // =====================================================
    // LOAD
    // =====================================================

    public void LoadEverything()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        cachedData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));

        SaveData data = cachedData;

        // BUILDINGS
        foreach (BuildingData b in data.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null) continue;

            GameObject prefab =
                (item is buildSO build) ? build.placeablePrefab :
                (item is SeedSO seed) ? seed.plantPrefab : null;

            if (prefab == null) continue;

            GameObject obj = Instantiate(prefab, b.position, Quaternion.identity);
            obj.name = prefab.name;

            if (obj.TryGetComponent(out ISaveableBuilding saveable))
                saveable.LoadSaveData(b.currentAmmo, b.plantProgress, b.structuralDurability);

            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                campfire.fuelAmount = b.fuelAmount;
                campfire.isBurning = b.isBurning;

                if (b.fuelItemID != -1)
                    campfire.fuelItem = database.GetItemByID(b.fuelItemID);

                campfire.SendMessage("UpdateVisuals", SendMessageOptions.DontRequireReceiver);
            }

            RegisterBuilding(obj);
        }

        // NPCS
        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
        {
            var npcSave = data.npcInventories.Find(x => x.npcId == npc.npcId);
            if (npcSave != null)
                npc.LoadFromSave(npcSave, database);
        }

        // BUSHES
        foreach (BushBehav bush in FindObjectsByType<BushBehav>(FindObjectsSortMode.None))
        {
            var bushSave = data.bushes.Find(x => x.bushID == bush.UniqOverworldItemID);
            if (bushSave != null)
                bush.LoadData(bushSave);
        }

        // TREES (ONLY HERE WE SPAWN)
        LoadTrees(data);

        TiltManager soil = FindFirstObjectByType<TiltManager>();
        if (soil != null)
            soil.LoadSoilTiles(data.soilTiles);
    }

    // =====================================================
    // TREE LOADING (FIXED: NO DOUBLE SPAWN)
    // =====================================================

    private void LoadTrees(SaveData data)
    {
        foreach (TreeSaveData t in data.trees)
        {
            if (t.isCut)
            {
                GameObject stump = Instantiate(stumpPrefab, t.position, Quaternion.identity);

                if (stump.TryGetComponent(out TreeStumpRegrow regrow))
                    regrow.Setup(t.treeID, t.cutTime);
            }
            else
            {
                GameObject treeObj = Instantiate(treePrefab, t.position, Quaternion.identity);

                if (treeObj.TryGetComponent(out treeItemBehav tree))
                {
                    tree.UniqOverworldItemID = t.treeID;
                    tree.LoadData(t);
                }
            }
        }
    }

    public void SaveAfterChange()
    {
        SaveNow();
    }

    public SaveData GetSaveData()
    {
        return cachedData;
    }
}