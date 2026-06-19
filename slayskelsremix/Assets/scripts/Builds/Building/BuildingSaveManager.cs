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
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Application.persistentDataPath + "/buildings.json";
    }

    private void Start()
    {
        Debug.Log("[SAVE MANAGER] LoadEverything() called");
        LoadEverything();
    }

    public void RegisterBuilding(GameObject obj)
    {
        if (obj == null) return;

        if (!placedObjects.Contains(obj))
            placedObjects.Add(obj);
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

        // =========================
        // PLAYER
        // =========================
        if (player != null)
            data.playerPosition = player.position;

        placedObjects.RemoveAll(x => x == null);

        var needs = player.GetComponent<PlayerNeeds>();
        if (needs != null)
        {
            data.saturation = needs.saturation;
        }


        // =========================
        // BUILDINGS
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

            // =========================
            // CAMPFIRE SAVE
            // =========================
            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;

                b.fuelItemIDs = new List<int>();

                if (campfire.validFuelItems != null)
                {
                    foreach (var fuel in campfire.validFuelItems)
                    {
                        if (fuel != null)
                            b.fuelItemIDs.Add(fuel.itemID);
                    }
                }

                // Backward compatibility
                b.fuelItemID =
                    (campfire.validFuelItems != null &&
                     campfire.validFuelItems.Count > 0 &&
                     campfire.validFuelItems[0] != null)
                    ? campfire.validFuelItems[0].itemID
                    : -1;
            }

            data.buildings.Add(b);
        }


        // =========================
        // PLANTS
        // =========================
        var plants = FindObjectsByType<PlantBehav>(FindObjectsSortMode.None);

        foreach (var plant in plants)
        {
            if (plant == null) continue;

            data.plants.Add(plant.GetPlantSaveData());
        }


        // =========================
        // OTHER SYSTEMS
        // =========================
        foreach (NpcInvBrain npc in FindObjectsByType<NpcInvBrain>(FindObjectsSortMode.None))
            data.npcInventories.Add(npc.GetSaveData());

        foreach (BushBehav bush in FindObjectsByType<BushBehav>(FindObjectsSortMode.None))
            data.bushes.Add(bush.GetSaveData());

        foreach (leafbehav leaf in FindObjectsByType<leafbehav>(FindObjectsSortMode.None))
            data.leafdata.Add(leaf.GetSaveData());

        foreach (stoneWORLDobjectbehav stone in FindObjectsByType<stoneWORLDobjectbehav>(FindObjectsSortMode.None))
            data.stonedata.Add(stone.GetSaveData());

        // =========================
        // TREES
        // =========================
        HashSet<string> seenTreeIDs = new();

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
        // SOIL
        // =========================
        if (tiltManager != null)
            data.soilTiles = tiltManager.GetSaveData();

        // =====================================================
        // TIME SAVE
        // =====================================================
        var time = FindFirstObjectByType<DayNightCycle>();

        if (time != null)
        {
            time.GetTime(
                out data.totalTime,
                out data.rawTime,
                out data.daysPassed
            );
        }

        cachedData = data;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

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

        string json = File.ReadAllText(savePath);
        cachedData = JsonUtility.FromJson<SaveData>(json);


        // =========================
        // PLAYER
        // =========================
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            player.position = cachedData.playerPosition;
            var needs = player.GetComponent<PlayerNeeds>();
            if (needs != null)
            {
                needs.saturation = cachedData.saturation;
            }



            if (cc != null)
                cc.enabled = true;
        }

        // =========================
        // BUILDINGS
        // =========================
        foreach (var b in cachedData.buildings)
        {
            ItemData item = database.GetItemByID(b.itemID);

            if (item == null)
                continue;

            if (item is not buildSO build)
                continue;

            GameObject obj =
                Instantiate(build.placeablePrefab, b.position, Quaternion.identity);

            // =========================
            // GENERIC BUILDING LOAD
            // =========================
            if (obj.TryGetComponent(out ISaveableBuilding saveable))
            {
                saveable.LoadSaveData(
                    b.currentAmmo,
                    b.plantProgress,
                    b.structuralDurability
                );
            }

            // =========================
            // CAMPFIRE LOAD
            // =========================
            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                campfire.fuelAmount = b.fuelAmount;
                campfire.isBurning = b.isBurning;

                campfire.validFuelItems = new List<ItemData>();

                // NEW FORMAT
                if (b.fuelItemIDs != null && b.fuelItemIDs.Count > 0)
                {
                    foreach (int id in b.fuelItemIDs)
                    {
                        ItemData fuel = database.GetItemByID(id);

                        if (fuel != null)
                            campfire.validFuelItems.Add(fuel);
                    }
                }
                // OLD FORMAT FALLBACK
                else if (b.fuelItemID != -1)
                {
                    ItemData fuel = database.GetItemByID(b.fuelItemID);

                    if (fuel != null)
                        campfire.validFuelItems.Add(fuel);
                }
            }

            RegisterBuilding(obj);
        }

        // =========================
        // PLANTS
        // =========================
        foreach (var p in cachedData.plants)
        {
            GameObject prefab = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(x => x.name == p.plantPrefabName);

            if (prefab == null)
            {
                Debug.LogError("[PLANT LOAD] Missing prefab: " + p.plantPrefabName);
                continue;
            }

            GameObject plantObj =
                Instantiate(prefab, p.position, Quaternion.identity);

            if (plantObj.TryGetComponent(out PlantBehav plant))
            {
                plant.LoadPlantSaveData(p);
            }
        }

        // =========================
        // NPC INVENTORIES
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
                .FirstOrDefault(bu => bu.UniqOverworldItemID == s.uniqID);

            if (existing != null)
            {
                existing.LoadData(s);
            }
            else if (bushPrefab != null)
            {
                var newBush =
                    Instantiate(bushPrefab, s.position, Quaternion.identity);

                if (newBush.TryGetComponent(out BushBehav b))
                    b.LoadData(s);
            }
        }

        // =========================
        // LEAVES
        // =========================
        foreach (LeafData l in cachedData.leafdata)
        {
            if (l.destroyed)
                continue;

            var leafObj =
                Instantiate(leafPrefab, l.position, Quaternion.identity);

            if (leafObj.TryGetComponent(out leafbehav leaf))
                leaf.LoadData(l);
        }

        // =========================
        // STONES
        // =========================
        foreach (stonedata s in cachedData.stonedata)
        {
            if (s.destroyed)
                continue;

            var stoneObj =
                Instantiate(stonePrefab, s.position, Quaternion.identity);

            if (stoneObj.TryGetComponent(out stoneWORLDobjectbehav stone))
                stone.LoadData(s);
        }

        // =========================
        // TREES
        // =========================
        HashSet<string> spawnedTreeIDs = new();

        foreach (TreeSaveData t in cachedData.trees)
        {
            if (!spawnedTreeIDs.Add(t.treeID.ToString()))
                continue;

            if (t.isCut)
            {
                var stump =
                    Instantiate(stumpPrefab, t.position, Quaternion.identity);

                if (stump.TryGetComponent(out TreeStumpRegrow r))
                    r.Setup(t.treeID, t.cutTime);
            }
            else
            {
                var treeObj =
                    Instantiate(treePrefab, t.position, Quaternion.identity);

                if (treeObj.TryGetComponent(out treeItemBehav tr))
                {
                    tr.LoadData(t);
                    tr.OnSpawnFromWorld();
                }
            }
        }

        // =========================
        // SOIL
        // =========================
        if (tiltManager != null && cachedData.soilTiles != null)
            tiltManager.LoadSoilTiles(cachedData.soilTiles);

        // =====================================================
        // TIME LOAD
        // =====================================================
        var time = FindFirstObjectByType<DayNightCycle>();

        if (time != null)
        {
            time.LoadTime(
                cachedData.totalTime,
                cachedData.rawTime,
                cachedData.daysPassed
            );
        }

        HasLoadedWorld = true;
        OnWorldLoaded?.Invoke();

        Debug.Log("========== LOAD END ==========");
    }

    public void SaveAfterChange() => SaveNow();

    public SaveData GetSaveData() => cachedData;
}