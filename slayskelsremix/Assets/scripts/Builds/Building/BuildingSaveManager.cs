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

    [Header("Tilemap Saving")]
    public TiltManager tiltManager;

    private List<GameObject> placedBuildings = new List<GameObject>();
    private string savePath;
    private SaveData cachedData;

    public static bool HasLoadedWorld = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/buildings.json";

        Debug.Log($"[SAVE MANAGER] Awake. Instance={GetInstanceID()} Path={savePath}");
    }

    private void Start()
    {
        Debug.Log("[SAVE MANAGER] Start -> LoadEverything()");
        LoadEverything();
    }

    public void RegisterBuilding(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("[REGISTER] NULL building attempted");
            return;
        }

        if (!placedBuildings.Contains(obj))
            placedBuildings.Add(obj);
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null)
            placedBuildings.Remove(obj);
    }

    public void SaveNow()
    {
        SaveData data = new SaveData();

        placedBuildings.RemoveAll(x => x == null);

        foreach (GameObject obj in placedBuildings)
        {
            if (obj == null) continue;

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

        HashSet<string> seenTreeIDs = new HashSet<string>();

        foreach (treeItemBehav tree in FindObjectsByType<treeItemBehav>(FindObjectsSortMode.None))
        {
            var t = tree.GetSaveData();
            if (!seenTreeIDs.Add(t.treeID.ToString())) continue;
            data.trees.Add(t);
        }

        foreach (TreeStumpRegrow stump in FindObjectsByType<TreeStumpRegrow>(FindObjectsSortMode.None))
        {
            if (!seenTreeIDs.Add(stump.treeID.ToString())) continue;

            data.trees.Add(new TreeSaveData
            {
                treeID = stump.treeID,
                isCut = true,
                cutTime = stump.GetCutTime(),
                position = stump.transform.position
            });
        }

        if (tiltManager != null)
        {
            data.soilTiles = tiltManager.GetSaveData();
            Debug.Log($"[SAVE][SOIL] tiles collected = {data.soilTiles?.Count ?? -1}");
        }
        else
        {
            Debug.LogError("[SAVE][SOIL] tiltManager is NULL");
        }

        cachedData = data;
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

        Debug.Log("[SAVE] Complete");
    }

    public void LoadEverything()
    {
        Debug.Log($"[LOAD] File exists = {File.Exists(savePath)}");

        if (!File.Exists(savePath))
        {
            HasLoadedWorld = true;
            OnWorldLoaded?.Invoke();
            Debug.Log("[LOAD] No save file found -> exit");
            return;
        }

        string json = File.ReadAllText(savePath);
        cachedData = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[LOAD] JSON parsed. soilTiles null? {cachedData.soilTiles == null}");
        Debug.Log($"[LOAD] soilTiles count = {cachedData.soilTiles?.Count ?? -1}");

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

        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
        {
            var s = cachedData.npcInventories.Find(x => x.npcId == npc.npcId);
            if (s != null) npc.LoadFromSave(s, database);
        }

        foreach (BushSaveData s in cachedData.bushes)
        {
            var existing = FindObjectsByType<BushBehav>(FindObjectsSortMode.None)
                .FirstOrDefault(b => b.UniqOverworldItemID == s.uniqID);

            if (existing != null)
                existing.LoadData(s);
            else if (bushPrefab != null)
            {
                GameObject newBush = Instantiate(bushPrefab, s.position, Quaternion.identity);
                if (newBush.TryGetComponent(out BushBehav b))
                    b.LoadData(s);
            }
        }

        HashSet<string> spawnedTreeIDs = new HashSet<string>();

        foreach (TreeSaveData t in cachedData.trees)
        {
            if (!spawnedTreeIDs.Add(t.treeID.ToString()))
                continue;

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
                    tr.LoadData(t);
            }
        }

        // 🔥 SOIL LOAD DEBUG BLOCK
        Debug.Log($"[LOAD][SOIL] tiltManager null? {tiltManager == null}");

        if (tiltManager != null && cachedData.soilTiles != null)
        {
            Debug.Log($"[LOAD][SOIL] CALLING LoadSoilTiles with {cachedData.soilTiles.Count} tiles");

            tiltManager.LoadSoilTiles(cachedData.soilTiles);

            Debug.Log("[LOAD][SOIL] LoadSoilTiles call finished");
        }
        else
        {
            Debug.LogError("[LOAD][SOIL] SKIPPED soil load (null manager or null data)");
        }

        HasLoadedWorld = true;
        OnWorldLoaded?.Invoke();

        Debug.Log("[LOAD] World load complete");
    }

    public void SaveAfterChange() => SaveNow();
    public SaveData GetSaveData() => cachedData;
}