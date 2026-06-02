using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance;

    public static event Action OnWorldLoaded;

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

    [Header("Databases & Prefabs")]
    public ItemDatabase database;
    public GameObject treePrefab;
    public GameObject stumpPrefab;
    public GameObject bushPrefab;

    private List<GameObject> placedBuildings = new List<GameObject>();
    private string savePath;
    private SaveData cachedData;

    public static bool HasLoadedWorld = false;

    // DEBUG COUNTERS
    private int saveBuildingCount;
    private int loadBuildingCount;
    private int skippedBuildingCount;

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
    // REGISTRATION
    // =====================================================
    public void RegisterBuilding(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("[REGISTER] NULL building attempted");
            return;
        }

        if (!placedBuildings.Contains(obj))
        {
            placedBuildings.Add(obj);
            Debug.Log($"[REGISTER] Added: {obj.name}");
        }
        else
        {
            Debug.LogWarning($"[REGISTER] Already exists: {obj.name}");
        }
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null && placedBuildings.Contains(obj))
        {
            placedBuildings.Remove(obj);
            Debug.Log($"[UNREGISTER] Removed: {obj.name}");
        }
    }

    // =====================================================
    // SAVE
    // =====================================================
    public void SaveNow()
    {
        SaveData data = new SaveData();

        placedBuildings.RemoveAll(x => x == null);

        // ================= BUILDINGS =================
        saveBuildingCount = 0;
        skippedBuildingCount = 0;

        Debug.Log($"[SAVE][BUILDINGS] Start | Count={placedBuildings.Count}");

        foreach (GameObject obj in placedBuildings)
        {
            if (obj == null)
            {
                Debug.LogWarning("[SAVE][BUILDINGS] NULL object skipped");
                skippedBuildingCount++;
                continue;
            }

            Debug.Log($"[SAVE][BUILDINGS] Processing: {obj.name} @ {obj.transform.position}");

            BuildingData b = new BuildingData
            {
                position = obj.transform.position
            };

            if (obj.TryGetComponent(out ISaveableBuilding saveable))
            {
                b.itemID = saveable.GetItemID();
                saveable.GetSaveData(out b.currentAmmo, out b.plantProgress, out b.structuralDurability);

                Debug.Log($"[SAVE][BUILDINGS] ISaveable OK | ID={b.itemID}, ammo={b.currentAmmo}, plant={b.plantProgress}, dura={b.structuralDurability}");
            }
            else
            {
                Debug.LogWarning($"[SAVE][BUILDINGS] Missing ISaveableBuilding: {obj.name}");
            }

            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;
                b.fuelItemID = campfire.fuelItem != null ? campfire.fuelItem.itemID : -1;

                Debug.Log($"[SAVE][BUILDINGS] Campfire state saved | fuel={b.fuelAmount}, burning={b.isBurning}");
            }

            data.buildings.Add(b);
            saveBuildingCount++;
        }

        Debug.Log($"[SAVE][BUILDINGS] Done | Saved={saveBuildingCount}, Skipped={skippedBuildingCount}");

        // NPC
        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
            data.npcInventories.Add(npc.GetSaveData());

        // Bushes
        foreach (BushBehav bush in FindObjectsByType<BushBehav>(FindObjectsSortMode.None))
            data.bushes.Add(bush.GetSaveData());

        // Trees
        HashSet<string> seenTreeIDs = new HashSet<string>();

        foreach (treeItemBehav tree in FindObjectsByType<treeItemBehav>(FindObjectsSortMode.None))
        {
            var t = tree.GetSaveData();

            if (!seenTreeIDs.Add(t.treeID.ToString()))
            {
                Debug.LogWarning($"[SAVE][TREE] Duplicate skipped: {t.treeID}");
                continue;
            }

            data.trees.Add(t);
        }

        foreach (TreeStumpRegrow stump in FindObjectsByType<TreeStumpRegrow>(FindObjectsSortMode.None))
        {
            if (!seenTreeIDs.Add(stump.treeID.ToString()))
            {
                Debug.LogWarning($"[SAVE][STUMP] Duplicate skipped: {stump.treeID}");
                continue;
            }

            data.trees.Add(new TreeSaveData
            {
                treeID = stump.treeID,
                isCut = true,
                cutTime = stump.GetCutTime(),
                position = stump.transform.position
            });
        }

        cachedData = data;
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

        Debug.Log("BuildingSaveManager: Save complete.");
    }

    // =====================================================
    // LOAD
    // =====================================================
    public void LoadEverything()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file found.");
            HasLoadedWorld = true;
            OnWorldLoaded?.Invoke();
            return;
        }

        Debug.Log("Loading: " + savePath);
        cachedData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));

        // ================= BUILDINGS =================
        loadBuildingCount = 0;

        Debug.Log($"[LOAD][BUILDINGS] Start | Count={cachedData.buildings.Count}");

        foreach (BuildingData b in cachedData.buildings)
        {
            Debug.Log($"[LOAD][BUILDINGS] Entry | ID={b.itemID}, Pos={b.position}");

            ItemData item = database.GetItemByID(b.itemID);

            if (item == null)
            {
                Debug.LogError($"[LOAD][BUILDINGS] Missing ItemID: {b.itemID}");
                continue;
            }

            GameObject prefab =
                (item is buildSO build) ? build.placeablePrefab :
                (item is SeedSO seed) ? seed.plantPrefab :
                null;

            if (prefab == null)
            {
                Debug.LogError($"[LOAD][BUILDINGS] Missing prefab for ID {b.itemID}");
                continue;
            }

            GameObject obj = Instantiate(prefab, b.position, Quaternion.identity);

            if (obj == null)
            {
                Debug.LogError("[LOAD][BUILDINGS] Instantiate failed");
                continue;
            }

            Debug.Log($"[LOAD][BUILDINGS] Spawned: {obj.name}");

            if (obj.TryGetComponent(out ISaveableBuilding s))
            {
                s.LoadSaveData(b.currentAmmo, b.plantProgress, b.structuralDurability);
            }
            else
            {
                Debug.LogWarning($"[LOAD][BUILDINGS] Missing ISaveableBuilding on {obj.name}");
            }

            RegisterBuilding(obj);
            loadBuildingCount++;
        }

        Debug.Log($"[LOAD][BUILDINGS] Done | Loaded={loadBuildingCount}");

        // NPC
        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
        {
            var s = cachedData.npcInventories.Find(x => x.npcId == npc.npcId);
            if (s != null) npc.LoadFromSave(s, database);
        }

        // Bushes
        foreach (BushSaveData s in cachedData.bushes)
        {
            var existingBush = FindObjectsByType<BushBehav>(FindObjectsSortMode.None)
                .FirstOrDefault(b => b.UniqOverworldItemID == s.uniqID);

            if (existingBush != null)
            {
                existingBush.LoadData(s);
            }
            else if (bushPrefab != null)
            {
                GameObject newBush = Instantiate(bushPrefab, s.position, Quaternion.identity);

                if (newBush.TryGetComponent(out BushBehav b))
                {
                    b.UniqOverworldItemID = s.uniqID;
                    b.LoadData(s);
                }
            }
        }

        // Trees
        HashSet<string> spawnedTreeIDs = new HashSet<string>();
        int duplicateCount = 0;

        foreach (TreeSaveData t in cachedData.trees)
        {
            if (!spawnedTreeIDs.Add(t.treeID.ToString()))
            {
                duplicateCount++;
                Debug.LogWarning($"[LOAD][TREE] Duplicate skipped: {t.treeID}");
                continue;
            }

            if (t.isCut)
            {
                GameObject stump = Instantiate(stumpPrefab, t.position, Quaternion.identity);
                if (stump.TryGetComponent(out TreeStumpRegrow r))
                    r.Setup(t.treeID, t.cutTime);
            }
            else
            {
                GameObject treeObj = Instantiate(treePrefab, t.position, Quaternion.identity);

                if (treeObj.TryGetComponent(out treeItemBehav tr))
                {
                    tr.UniqOverworldItemID = t.treeID;
                    tr.LoadData(t);
                }
            }
        }

        if (duplicateCount > 0)
            Debug.LogWarning($"[LOAD] Duplicate trees skipped: {duplicateCount}");

        HasLoadedWorld = true;
        Debug.Log("World load complete");
        OnWorldLoaded?.Invoke();
    }

    public void SaveAfterChange() => SaveNow();
    public SaveData GetSaveData() => cachedData;
}