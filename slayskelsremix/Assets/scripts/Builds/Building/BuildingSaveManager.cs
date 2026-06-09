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

    [Header("Databases & Prefabs")]
    public ItemDatabase database;

    public GameObject treePrefab;
    public GameObject stumpPrefab;
    public GameObject bushPrefab;
    public GameObject leafPrefab;
    public GameObject stonePrefab;

    [Header("Tilemap Saving")]
    public TiltManager tiltManager;

    private readonly List<GameObject> placedObjects = new();

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

    public void RegisterBuilding(GameObject obj)
    {
        if (obj == null) return;
        if (!placedObjects.Contains(obj)) placedObjects.Add(obj);
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj == null) return;
        placedObjects.Remove(obj);
    }

    // =====================================================
    // SAVE
    // =====================================================

    public void SaveNow()
    {
        SaveData data = new SaveData();

        if (player != null)
            data.playerPosition = player.position;

        placedObjects.RemoveAll(x => x == null);

        // =========================
        // BUILDINGS + PLANTS (UNIFIED SYSTEM)
        // =========================
        foreach (GameObject obj in placedObjects)
        {
            if (!obj.TryGetComponent(out ISaveableBuilding saveable))
                continue;

            BuildingData b = new BuildingData
            {
                position = obj.transform.position,
                itemID = saveable.GetItemID()
            };

            saveable.GetSaveData(
                out b.currentAmmo,
                out b.plantProgress,
                out b.structuralDurability
            );

            data.buildings.Add(b);
        }

        // =========================
        // NPCs
        // =========================
        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
            data.npcInventories.Add(npc.GetSaveData());

        // =========================
        // BUSHES
        // =========================
        foreach (BushBehav bush in FindObjectsByType<BushBehav>(FindObjectsSortMode.None))
            data.bushes.Add(bush.GetSaveData());

        // =========================
        // LEAVES
        // =========================
        foreach (leafbehav leaf in FindObjectsByType<leafbehav>(FindObjectsSortMode.None))
            data.leafdata.Add(leaf.GetSaveData());

        // =========================
        // STONES
        // =========================
        foreach (stoneWORLDobjectbehav stone in FindObjectsByType<stoneWORLDobjectbehav>(FindObjectsSortMode.None))
            data.stonedata.Add(stone.GetSaveData());

        // =========================
        // TREES
        // =========================
        HashSet<string> seenTreeIDs = new();

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
            data.soilTiles = tiltManager.GetSaveData();

        cachedData = data;

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

        Debug.Log("WORLD SAVED");
    }

    // =====================================================
    // LOAD
    // =====================================================

    public void LoadEverything()
    {
        if (!File.Exists(savePath))
        {
            HasLoadedWorld = true;
            OnWorldLoaded?.Invoke();
            return;
        }

        cachedData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));

        // =========================
        // PLAYER
        // =========================
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.position = cachedData.playerPosition;

            if (cc != null) cc.enabled = true;
        }

        // =========================
        // BUILDINGS + PLANTS
        // =========================
        foreach (var b in cachedData.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);
            if (item == null) continue;

            if (item is not buildSO build) continue;

            GameObject obj = Instantiate(build.placeablePrefab, b.position, Quaternion.identity);

            if (obj.TryGetComponent(out ISaveableBuilding saveable))
            {
                saveable.LoadSaveData(
                    b.currentAmmo,
                    b.plantProgress,
                    b.structuralDurability
                );
            }

            RegisterBuilding(obj);
        }

        // =========================
        // NPCs
        // =========================
        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
        {
            var save = cachedData.npcInventories
                .Find(x => x.npcId == npc.npcId);

            if (save != null)
                npc.LoadFromSave(save, database);
        }

        // =========================
        // BUSHES
        // =========================
        foreach (BushSaveData s in cachedData.bushes)
        {
            BushBehav existing =
                FindObjectsByType<BushBehav>(FindObjectsSortMode.None)
                .FirstOrDefault(b => b.UniqOverworldItemID == s.uniqID);

            if (existing != null) existing.LoadData(s);
            else if (bushPrefab != null)
            {
                var newBush = Instantiate(bushPrefab, s.position, Quaternion.identity);
                if (newBush.TryGetComponent(out BushBehav b)) b.LoadData(s);
            }
        }

        // =========================
        // LEAVES
        // =========================
        foreach (LeafData l in cachedData.leafdata)
        {
            if (l.destroyed) continue;

            var leafObj = Instantiate(leafPrefab, l.position, Quaternion.identity);
            if (leafObj.TryGetComponent(out leafbehav leaf)) leaf.LoadData(l);
        }

        // =========================
        // STONES
        // =========================
        foreach (stonedata s in cachedData.stonedata)
        {
            if (s.destroyed) continue;

            var stoneObj = Instantiate(stonePrefab, s.position, Quaternion.identity);
            if (stoneObj.TryGetComponent(out stoneWORLDobjectbehav stone))
                stone.LoadData(s);
        }

        // =========================
        // TREES
        // =========================
        HashSet<string> spawnedTreeIDs = new();

        foreach (TreeSaveData t in cachedData.trees)
        {
            if (!spawnedTreeIDs.Add(t.treeID.ToString())) continue;

            if (t.isCut)
            {
                var stump = Instantiate(stumpPrefab, t.position, Quaternion.identity);
                if (stump.TryGetComponent(out TreeStumpRegrow r))
                    r.Setup(t.treeID, t.cutTime);
            }
            else
            {
                var treeObj = Instantiate(treePrefab, t.position, Quaternion.identity);
                if (treeObj.TryGetComponent(out treeItemBehav tr))
                {
                    tr.LoadData(t);
                    tr.OnSpawnFromWorld();
                }
            }
        }

        if (tiltManager != null && cachedData.soilTiles != null)
            tiltManager.LoadSoilTiles(cachedData.soilTiles);

        HasLoadedWorld = true;
        OnWorldLoaded?.Invoke();

        Debug.Log("WORLD LOADED");
    }

    public void SaveAfterChange() => SaveNow();


    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    public SaveData GetSaveData()
    {
        return cachedData;
    }


}


