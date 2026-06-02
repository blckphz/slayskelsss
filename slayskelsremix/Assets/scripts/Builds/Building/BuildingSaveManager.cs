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
        if (obj != null && !placedBuildings.Contains(obj))
            placedBuildings.Add(obj);
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null && placedBuildings.Contains(obj))
            placedBuildings.Remove(obj);
    }

    // =====================================================
    // SAVE
    // =====================================================
    public void SaveNow()
    {
        SaveData data = new SaveData();
        placedBuildings.RemoveAll(x => x == null);

        // ---------------- BUILDINGS ----------------
        foreach (GameObject obj in placedBuildings)
        {
            BuildingData b = new BuildingData
            {
                position = obj.transform.position
            };

            if (obj.TryGetComponent(out ISaveableBuilding saveable))
            {
                b.itemID = saveable.GetItemID();
                saveable.GetSaveData(out b.currentAmmo, out b.plantProgress, out b.structuralDurability);
            }

            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;
                b.fuelItemID = campfire.fuelItem != null ? campfire.fuelItem.itemID : -1;
            }

            data.buildings.Add(b);
        }

        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
            data.npcInventories.Add(npc.GetSaveData());

        foreach (BushBehav bush in FindObjectsByType<BushBehav>(FindObjectsSortMode.None))
            data.bushes.Add(bush.GetSaveData());

        // =====================================================
        // 🌳 TREE SAVE (PATCHED - DEDUP + DEBUG)
        // =====================================================
        HashSet<string> seenTreeIDs = new HashSet<string>();

        foreach (treeItemBehav tree in FindObjectsByType<treeItemBehav>(FindObjectsSortMode.None))
        {
            var t = tree.GetSaveData();

            if (!seenTreeIDs.Add(t.treeID.ToString()))
            {
                Debug.LogWarning($"[SAVE] Duplicate treeItemBehav skipped: {t.treeID} at {tree.transform.position}");
                continue;
            }

            data.trees.Add(t);
        }

        // =====================================================
        // 🌱 STUMP SAVE (PATCHED - DEDUP + DEBUG)
        // =====================================================
        foreach (TreeStumpRegrow stump in FindObjectsByType<TreeStumpRegrow>(FindObjectsSortMode.None))
        {
            if (!seenTreeIDs.Add(stump.treeID.ToString()))
            {
                Debug.LogWarning($"[SAVE] Duplicate stump skipped: {stump.treeID} at {stump.transform.position}");
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
            Debug.Log("BuildingSaveManager: No save file found. Initializing fresh world.");
            HasLoadedWorld = true;
            OnWorldLoaded?.Invoke();
            return;
        }

        Debug.Log("BuildingSaveManager: Loading data from " + savePath);
        cachedData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));

        // ---------------- BUILDINGS ----------------
        foreach (BuildingData b in cachedData.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null) continue;

            GameObject prefab =
                (item is buildSO build) ? build.placeablePrefab :
                (item is SeedSO seed) ? seed.plantPrefab :
                null;

            if (prefab == null) continue;

            GameObject obj = Instantiate(prefab, b.position, Quaternion.identity);

            if (obj.TryGetComponent(out ISaveableBuilding s))
                s.LoadSaveData(b.currentAmmo, b.plantProgress, b.structuralDurability);

            RegisterBuilding(obj);
        }

        // ---------------- NPC ----------------
        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
        {
            var s = cachedData.npcInventories.Find(x => x.npcId == npc.npcId);
            if (s != null) npc.LoadFromSave(s, database);
        }

        // ---------------- BUSHES ----------------
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

        // =====================================================
        // 🌳 TREE LOAD (PATCHED DEDUP + DEBUG)
        // =====================================================
        HashSet<string> spawnedTreeIDs = new HashSet<string>();
        int duplicateCount = 0;

        foreach (TreeSaveData t in cachedData.trees)
        {
            if (!spawnedTreeIDs.Add(t.treeID.ToString()))
            {
                duplicateCount++;
                Debug.LogWarning($"[LOAD] Duplicate tree skipped: {t.treeID} at {t.position}");
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
            Debug.LogWarning($"[LOAD] Total duplicate trees skipped: {duplicateCount}");

        HasLoadedWorld = true;
        Debug.Log("BuildingSaveManager: Loading complete. Invoking OnWorldLoaded.");
        OnWorldLoaded?.Invoke();
    }

    public void SaveAfterChange() => SaveNow();
    public SaveData GetSaveData() => cachedData;
}