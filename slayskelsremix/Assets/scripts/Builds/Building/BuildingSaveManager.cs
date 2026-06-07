using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance;
    public static event Action OnWorldLoaded;

    [Header("Player Reference")]
    public Transform player;

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
    public GameObject leafPrefab;
    public GameObject stonePrefab;

    [Header("Tilemap Saving")]
    public TiltManager tiltManager;

    private List<GameObject> placedBuildings = new List<GameObject>();

    private string savePath;
    private SaveData cachedData;

    public static bool HasLoadedWorld = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Application.persistentDataPath + "/buildings.json";
    }

    private void Start()
    {
        LoadEverything();
    }

    // =========================================================
    // SAVE FILE CHECK
    // =========================================================

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    // =========================================================
    // REGISTER BUILDINGS
    // =========================================================

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

    // =========================================================
    // SAVE
    // =========================================================

    public void SaveNow()
    {
        SaveData data = new SaveData();

        // =========================
        // PLAYER POSITION
        // =========================

        if (player != null)
        {
            data.playerPosition = player.position;
        }

        // =========================
        // BUILDINGS
        // =========================

        placedBuildings.RemoveAll(x => x == null);

        foreach (GameObject obj in placedBuildings)
        {
            if (obj == null)
                continue;

            BuildingData b = new BuildingData
            {
                position = obj.transform.position
            };

            if (obj.TryGetComponent(out ISaveableBuilding saveable))
            {
                b.itemID = saveable.GetItemID();

                saveable.GetSaveData(
                    out b.currentAmmo,
                    out b.plantProgress,
                    out b.structuralDurability
                );
            }

            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;

                b.fuelItemID =
                    campfire.fuelItem != null
                    ? campfire.fuelItem.itemID
                    : -1;
            }

            data.buildings.Add(b);
        }

        // =========================
        // NPC INVENTORIES
        // =========================

        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
        {
            data.npcInventories.Add(npc.GetSaveData());
        }

        // =========================
        // BUSHES
        // =========================

        foreach (BushBehav bush in FindObjectsByType<BushBehav>(FindObjectsSortMode.None))
        {
            data.bushes.Add(bush.GetSaveData());
        }

        // =========================
        // LEAVES
        // =========================

        foreach (leafbehav leaf in FindObjectsByType<leafbehav>(FindObjectsSortMode.None))
        {
            data.leafdata.Add(leaf.GetSaveData());
        }

        // =========================
        // STONES
        // =========================

        foreach (stoneWORLDobjectbehav stone in FindObjectsByType<stoneWORLDobjectbehav>(FindObjectsSortMode.None))
        {
            data.stonedata.Add(stone.GetSaveData());
        }

        // =========================
        // TREES
        // =========================

        HashSet<string> seenTreeIDs = new HashSet<string>();

        foreach (treeItemBehav tree in FindObjectsByType<treeItemBehav>(FindObjectsSortMode.None))
        {
            var t = tree.GetSaveData();

            if (!seenTreeIDs.Add(t.treeID.ToString()))
                continue;

            data.trees.Add(t);
        }

        foreach (TreeStumpRegrow stump in FindObjectsByType<TreeStumpRegrow>(FindObjectsSortMode.None))
        {
            if (!seenTreeIDs.Add(stump.treeID.ToString()))
                continue;

            data.trees.Add(new TreeSaveData
            {
                treeID = stump.treeID,
                isCut = true,
                cutTime = stump.GetCutTime(),
                position = stump.transform.position
            });
        }

        // =========================
        // SOIL TILES
        // =========================

        if (tiltManager != null)
        {
            data.soilTiles = tiltManager.GetSaveData();
        }

        // =========================
        // WRITE FILE
        // =========================

        cachedData = data;

        File.WriteAllText(
            savePath,
            JsonUtility.ToJson(data, true)
        );
    }

    // =========================================================
    // LOAD
    // =========================================================

    public void LoadEverything()
    {
        if (!File.Exists(savePath))
        {
            HasLoadedWorld = true;
            OnWorldLoaded?.Invoke();
            return;
        }

        string json = File.ReadAllText(savePath);

        cachedData = JsonUtility.FromJson<SaveData>(json);

        // =========================
        // PLAYER POSITION
        // =========================

        if (player != null)
        {
            CharacterController cc =
                player.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            player.position = cachedData.playerPosition;

            if (cc != null)
                cc.enabled = true;
        }

        // =========================
        // BUILDINGS
        // =========================

        foreach (BuildingData b in cachedData.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);

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

            GameObject obj = Instantiate(
                prefab,
                b.position,
                Quaternion.identity
            );

            if (obj.TryGetComponent(out ISaveableBuilding s))
            {
                s.LoadSaveData(
                    b.currentAmmo,
                    b.plantProgress,
                    b.structuralDurability
                );
            }

            RegisterBuilding(obj);
        }

        // =========================
        // NPC INVENTORIES
        // =========================

        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
        {
            var s = cachedData.npcInventories.Find(
                x => x.npcId == npc.npcId
            );

            if (s != null)
            {
                npc.LoadFromSave(s, database);
            }
        }

        // =========================
        // BUSHES
        // =========================

        foreach (BushSaveData s in cachedData.bushes)
        {
            var existing =
                FindObjectsByType<BushBehav>(FindObjectsSortMode.None)
                .FirstOrDefault(
                    b => b.UniqOverworldItemID == s.uniqID
                );

            if (existing != null)
            {
                existing.LoadData(s);
            }
            else if (bushPrefab != null)
            {
                GameObject newBush = Instantiate(
                    bushPrefab,
                    s.position,
                    Quaternion.identity
                );

                if (newBush.TryGetComponent(out BushBehav b))
                {
                    b.LoadData(s);
                }
            }
        }

        // =========================
        // LEAVES
        // =========================

        foreach (LeafData l in cachedData.leafdata)
        {
            if (l.destroyed)
                continue;

            GameObject leafObj = Instantiate(
                leafPrefab,
                l.position,
                Quaternion.identity
            );

            if (leafObj.TryGetComponent(out leafbehav leaf))
            {
                leaf.LoadData(l);
            }
        }

        // =========================
        // STONES
        // =========================

        foreach (stonedata s in cachedData.stonedata)
        {
            if (s.destroyed)
                continue;

            GameObject stoneObj = Instantiate(
                stonePrefab,
                s.position,
                Quaternion.identity
            );

            if (stoneObj.TryGetComponent(out stoneWORLDobjectbehav stone))
            {
                stone.LoadData(s);
            }
        }

        // =========================
        // TREES
        // =========================

        HashSet<string> spawnedTreeIDs = new HashSet<string>();

        foreach (TreeSaveData t in cachedData.trees)
        {
            if (!spawnedTreeIDs.Add(t.treeID.ToString()))
                continue;

            if (t.isCut)
            {
                GameObject stump = Instantiate(
                    stumpPrefab,
                    t.position,
                    Quaternion.identity
                );

                if (stump.TryGetComponent(out TreeStumpRegrow r))
                {
                    r.Setup(t.treeID, t.cutTime);
                }
            }
            else
            {
                GameObject treeObj = Instantiate(
                    treePrefab,
                    t.position,
                    Quaternion.identity
                );

                if (treeObj.TryGetComponent(out treeItemBehav tr))
                {
                    tr.LoadData(t);
                    tr.OnSpawnFromWorld();
                }
            }
        }

        // =========================
        // SOIL TILES
        // =========================

        if (tiltManager != null &&
            cachedData.soilTiles != null)
        {
            tiltManager.LoadSoilTiles(
                cachedData.soilTiles
            );
        }

        // =========================
        // FINISH
        // =========================

        HasLoadedWorld = true;

        OnWorldLoaded?.Invoke();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    public void SaveAfterChange()
    {
        SaveNow();
    }

    public SaveData GetSaveData()
    {
        return cachedData;
    }
}
