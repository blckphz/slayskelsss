using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance;

    // =====================================================
    // BUILDING DATA
    // =====================================================

    [System.Serializable]
    public class BuildingData
    {
        public int itemID;
        public string uniqueID;

        public Vector3 position;

        public int currentAmmo;
        public float plantProgress;
        public int structuralDurability;

        // CAMPFIRE
        public float fuelAmount;
        public bool isBurning;
        public int fuelItemID;
    }

    // =====================================================
    // ROOT SAVE FILE
    // =====================================================

    [System.Serializable]
    public class SaveData
    {
        // BUILDINGS
        public List<BuildingData> buildings =
            new List<BuildingData>();

        // NPCS
        public List<NpcInventorySaveData> npcInventories =
            new List<NpcInventorySaveData>();

        // BUSHES
        public List<BushSaveData> bushes =
            new List<BushSaveData>();

        // TREES
        public List<TreeSaveData> trees =
            new List<TreeSaveData>();

        // SOIL
        public List<SoilData> soilTiles =
            new List<SoilData>();
    }

    // =====================================================
    // REFERENCES
    // =====================================================

    public ItemDatabase database;

    private List<GameObject> placedBuildings =
        new List<GameObject>();

    private string savePath;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath =
            Application.persistentDataPath +
            "/buildings.json";
    }

    private void Start()
    {
        LoadEverything();
    }

    // =====================================================
    // REGISTER BUILDINGS
    // =====================================================

    public void RegisterBuilding(GameObject obj)
    {
        if (obj != null &&
            !placedBuildings.Contains(obj))
        {
            placedBuildings.Add(obj);
        }
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null &&
            placedBuildings.Contains(obj))
        {
            placedBuildings.Remove(obj);
        }
    }

    // =====================================================
    // SAVE EVERYTHING
    // =====================================================

    public void SaveNow()
    {
        SaveData data = new SaveData();

        // =================================================
        // 1. SAVE BUILDINGS
        // =================================================

        placedBuildings.RemoveAll(item => item == null);

        foreach (GameObject obj in placedBuildings)
        {
            BuildingData b = new BuildingData();

            b.position = obj.transform.position;

            // SAVEABLE BUILDINGS
            if (obj.TryGetComponent(
                out ISaveableBuilding saveable))
            {
                b.itemID = saveable.GetItemID();

                saveable.GetSaveData(
                    out b.currentAmmo,
                    out b.plantProgress,
                    out b.structuralDurability
                );
            }
            // NORMAL BUILD OBJECTS
            else if (obj.TryGetComponent(
                out BuildIdentity id))
            {
                if (id.item != null)
                {
                    b.itemID = id.item.itemID;
                }
            }

            // CAMPFIRE
            if (obj.TryGetComponent(
                out CampfireBehav campfire))
            {
                b.fuelAmount =
                    campfire.fuelAmount;

                b.isBurning =
                    campfire.isBurning;

                b.fuelItemID =
                    campfire.fuelItem != null
                    ? campfire.fuelItem.itemID
                    : -1;
            }

            data.buildings.Add(b);
        }

        // =================================================
        // 2. SAVE NPCS
        // =================================================

        NpcInvBrain[] npcs =
            FindObjectsByType<NpcInvBrain>(
                FindObjectsSortMode.None);

        foreach (NpcInvBrain npc in npcs)
        {
            data.npcInventories.Add(
                npc.GetSaveData());
        }

        // =================================================
        // 3. SAVE BUSHES
        // =================================================

        BushBehav[] bushes =
            FindObjectsByType<BushBehav>(
                FindObjectsSortMode.None);

        foreach (BushBehav bush in bushes)
        {
            data.bushes.Add(
                new BushSaveData
                {
                    bushID = bush.UniqOverworldItemID,
                    isHarvested = bush.IsHarvested,
                    health = bush.health,
                    timeAtHarvest =
                        bush.GetSaveData().timeAtHarvest
                });
        }

        // =================================================
        // 4. SAVE TREES
        // =================================================

        treeItemBehav[] trees =
            FindObjectsByType<treeItemBehav>(
                FindObjectsSortMode.None);

        foreach (treeItemBehav tree in trees)
        {
            data.trees.Add(
                tree.GetSaveData());
        }

        // SAVE STUMPS
        TreeStumpRegrow[] stumps =
            FindObjectsByType<TreeStumpRegrow>(
                FindObjectsSortMode.None);

        foreach (TreeStumpRegrow stump in stumps)
        {
            data.trees.Add(
                new TreeSaveData
                {
                    treeID = stump.treeID,
                    isCut = true,
                    cutTime = stump.GetCutTime()
                });
        }

        // =================================================
        // 5. SAVE SOIL
        // =================================================

        TiltManager tilt =
            FindFirstObjectByType<TiltManager>();

        if (tilt != null)
        {
            data.soilTiles =
                tilt.GetSaveData();
        }

        // =================================================
        // WRITE FILE
        // =================================================

        string json =
            JsonUtility.ToJson(data, true);

        File.WriteAllText(savePath, json);

        Debug.Log("[SaveManager] SAVE COMPLETE");
    }

    // =====================================================
    // LOAD EVERYTHING
    // =====================================================

    public void LoadEverything()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning(
                "No save file found.");

            return;
        }

        string json =
            File.ReadAllText(savePath);

        SaveData data =
            JsonUtility.FromJson<SaveData>(json);

        // =================================================
        // 1. LOAD BUILDINGS
        // =================================================

        foreach (BuildingData b in data.buildings)
        {
            ItemData item =
                database.GetItemByID(b.itemID);

            if (item == null)
                continue;

            GameObject prefab =
                (item is buildSO build)
                    ? build.placeablePrefab
                    : (item is SeedSO seed)
                        ? seed.plantPrefab
                        : null;

            if (prefab == null)
                continue;

            GameObject obj =
                Instantiate(
                    prefab,
                    b.position,
                    Quaternion.identity);

            obj.name = prefab.name;

            // LOAD SAVEABLE BUILDINGS
            if (obj.TryGetComponent(
                out ISaveableBuilding saveable))
            {
                saveable.LoadSaveData(
                    b.currentAmmo,
                    b.plantProgress,
                    b.structuralDurability
                );
            }

            // LOAD CAMPFIRE
            if (obj.TryGetComponent(
                out CampfireBehav campfire))
            {
                campfire.fuelAmount =
                    b.fuelAmount;

                campfire.isBurning =
                    b.isBurning;

                if (b.fuelItemID != -1)
                {
                    campfire.fuelItem =
                        database.GetItemByID(
                            b.fuelItemID);
                }

                campfire.SendMessage(
                    "UpdateVisuals",
                    SendMessageOptions
                        .DontRequireReceiver);
            }

            RegisterBuilding(obj);
        }

        // =================================================
        // 2. LOAD NPCS
        // =================================================

        foreach (NpcInvBrain npc in
            FindObjectsByType<NpcInvBrain>(
                FindObjectsSortMode.None))
        {
            var npcSave =
                data.npcInventories.Find(
                    x => x.npcId == npc.npcId);

            if (npcSave != null)
            {
                npc.LoadFromSave(
                    npcSave,
                    database);
            }
        }

        // =================================================
        // 3. LOAD BUSHES
        // =================================================

        foreach (BushBehav bush in
            FindObjectsByType<BushBehav>(
                FindObjectsSortMode.None))
        {
            var bushSave =
                data.bushes.Find(
                    x => x.bushID == bush.UniqOverworldItemID);

            if (bushSave != null)
            {
                bush.LoadData(bushSave);
            }
        }

        // =================================================
        // 4. LOAD TREES
        // =================================================

        foreach (treeItemBehav tree in
            FindObjectsByType<treeItemBehav>(
                FindObjectsSortMode.None))
        {
            var treeSave =
                data.trees.Find(
                    x => x.treeID ==
                    tree.UniqOverworldItemID);

            if (treeSave != null)
            {
                tree.LoadData(treeSave);
            }
        }

        // =================================================
        // 5. LOAD SOIL
        // =================================================

        TiltManager tilt =
            FindFirstObjectByType<TiltManager>();

        if (tilt != null)
        {
            tilt.LoadSoilTiles(
                data.soilTiles);
        }

        Debug.Log(
            "World loaded successfully.");
    }

    // =====================================================
    // QUICK SAVE
    // =====================================================

    public void SaveAfterChange()
    {
        SaveNow();
    }
}