using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance;

    public static event Action OnWorldLoaded;


    // =====================================================
    // PLAYER
    // =====================================================

    [Header("Player Reference")]
    public Transform player;


    // =====================================================
    // GRID
    // =====================================================

    [Header("Grid Reference")]
    public Grid buildGrid;


    // =====================================================
    // DATABASE
    // =====================================================

    [Header("Databases & Prefabs")]
    public ItemDatabase database;

    public GameObject treePrefab;
    public GameObject stumpPrefab;
    public GameObject bushPrefab;
    public GameObject leafPrefab;
    public GameObject stonePrefab;


    // =====================================================
    // SOIL
    // =====================================================

    [Header("Tilemap Saving")]
    public TiltManager tiltManager;


    // =====================================================
    // BUILD MANAGER
    // =====================================================

    [Header("Build Manager")]
    public BuildManager buildManager;


    // =====================================================
    // TILE SAVER
    // =====================================================

    [Header("Base Tilemaps")]
    public TileSaver tileSaver;


    // =====================================================
    // ROOM SYSTEM
    // =====================================================

    [Header("Room / Structure System")]
    public BaseTilePlacementManager
        baseTilePlacementManager;


    // =====================================================
    // BUILDINGS
    // =====================================================

    private readonly List<GameObject> placedObjects =
        new List<GameObject>();


    // =====================================================
    // SAVE PATH
    // =====================================================

    private string savePath;


    // =====================================================
    // CACHED DATA
    // =====================================================

    private SaveData cachedData;


    // =====================================================
    // LOAD STATE
    // =====================================================

    public static bool HasLoadedWorld = false;


    // =====================================================
    // AWAKE
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


        // =================================================
        // AUTO FIND ROOM SYSTEM
        // =================================================

        if (
            baseTilePlacementManager == null)
        {
            baseTilePlacementManager =
                BaseTilePlacementManager.Instance;
        }
    }


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        LoadEverything();
    }


    // =====================================================
    // REGISTER BUILDING
    // =====================================================

    public void RegisterBuilding(
        GameObject obj)
    {
        if (obj == null)
            return;


        if (!placedObjects.Contains(obj))
        {
            placedObjects.Add(obj);
        }
    }


    // =====================================================
    // UNREGISTER BUILDING
    // =====================================================

    public void UnregisterBuilding(
        GameObject obj)
    {
        if (obj == null)
            return;


        placedObjects.Remove(obj);
    }


    // =====================================================
    // SAVE NOW
    // =====================================================

    public void SaveNow()
    {
        SaveData data =
            new SaveData();


        // =================================================
        // PLAYER
        // =================================================

        if (player != null)
        {
            data.playerPosition =
                player.position;


            PlayerNeeds needs =
                player.GetComponent<PlayerNeeds>();


            if (needs != null)
            {
                data.saturation =
                    needs.saturation;
            }
        }


        // =================================================
        // REMOVE NULL BUILDINGS
        // =================================================

        placedObjects.RemoveAll(
            x => x == null
        );


        // =================================================
        // BUILDINGS
        // =================================================

        foreach (
            GameObject obj
            in placedObjects)
        {
            if (!obj.TryGetComponent(
                out ISaveableBuilding saveable))
            {
                continue;
            }


            BuildingData b =
                new BuildingData
                {
                    position =
                        obj.transform.position,

                    itemID =
                        saveable.GetItemID()
                };


            saveable.GetSaveData(
                out b.currentAmmo,
                out b.plantProgress,
                out b.structuralDurability
            );


            // =================================================
            // CAMPFIRE
            // =================================================

            if (obj.TryGetComponent(
                out CampfireBehav campfire))
            {
                b.fuelAmount =
                    campfire.fuelAmount;


                b.isBurning =
                    campfire.isBurning;


                b.fuelItemIDs =
                    new List<int>();


                if (
                    campfire.validFuelItems != null)
                {
                    foreach (
                        ItemData fuel
                        in campfire.validFuelItems)
                    {
                        if (fuel != null)
                        {
                            b.fuelItemIDs.Add(
                                fuel.itemID
                            );
                        }
                    }
                }


                b.fuelItemID =
                    (
                        campfire.validFuelItems != null &&
                        campfire.validFuelItems.Count > 0 &&
                        campfire.validFuelItems[0] != null
                    )
                    ? campfire.validFuelItems[0].itemID
                    : -1;
            }


            data.buildings.Add(
                b
            );
        }


        // =================================================
        // PLANTS
        // =================================================

        foreach (
            PlantBehav plant
            in FindObjectsByType<PlantBehav>(
                FindObjectsSortMode.None))
        {
            if (plant == null)
                continue;


            data.plants.Add(
                plant.GetPlantSaveData()
            );
        }


        // =================================================
        // NPC INVENTORIES
        // =================================================

        foreach (
            NpcInvBrain npc
            in FindObjectsByType<NpcInvBrain>(
                FindObjectsSortMode.None))
        {
            if (npc == null)
                continue;


            data.npcInventories.Add(
                npc.GetSaveData()
            );
        }


        // =================================================
        // BUSHES
        // =================================================

        foreach (
            BushBehav bush
            in FindObjectsByType<BushBehav>(
                FindObjectsSortMode.None))
        {
            if (bush == null)
                continue;


            data.bushes.Add(
                bush.GetSaveData()
            );
        }


        // =================================================
        // LEAVES
        // =================================================

        foreach (
            leafbehav leaf
            in FindObjectsByType<leafbehav>(
                FindObjectsSortMode.None))
        {
            if (leaf == null)
                continue;


            data.leafdata.Add(
                leaf.GetSaveData()
            );
        }


        // =================================================
        // STONES
        // =================================================

        foreach (
            stoneWORLDobjectbehav stone
            in FindObjectsByType<
                stoneWORLDobjectbehav>(
                FindObjectsSortMode.None))
        {
            if (stone == null)
                continue;


            data.stonedata.Add(
                stone.GetSaveData()
            );
        }


        // =================================================
        // TREES
        // =================================================

        HashSet<string> seenTreeIDs =
            new HashSet<string>();


        foreach (
            treeItemBehav tree
            in FindObjectsByType<
                treeItemBehav>(
                FindObjectsSortMode.None))
        {
            if (tree == null)
                continue;


            TreeSaveData treeData =
                tree.GetSaveData();


            if (!seenTreeIDs.Add(
                treeData.treeID.ToString()))
            {
                continue;
            }


            data.trees.Add(
                treeData
            );
        }


        foreach (
            TreeStumpRegrow stump
            in FindObjectsByType<
                TreeStumpRegrow>(
                FindObjectsSortMode.None))
        {
            if (stump == null)
                continue;


            if (!seenTreeIDs.Add(
                stump.treeID.ToString()))
            {
                continue;
            }


            data.trees.Add(
                new TreeSaveData
                {
                    treeID =
                        stump.treeID,

                    isCut =
                        true,

                    cutTime =
                        stump.GetCutTime(),

                    position =
                        stump.transform.position
                }
            );
        }


        // =================================================
        // SOIL
        // =================================================

        if (tiltManager != null)
        {
            data.soilTiles =
                tiltManager.GetSaveData();
        }


        // =================================================
        // BASE TILEMAPS
        // =================================================

        SaveBaseTiles(
            data
        );


        // =================================================
        // STRUCTURES
        // =================================================

        SaveBuildingStructures(
            data
        );


        // =================================================
        // TIME
        // =================================================

        DayNightCycle time =
            FindFirstObjectByType<DayNightCycle>();


        if (time != null)
        {
            time.GetTime(
                out data.totalTime,
                out data.rawTime,
                out data.daysPassed
            );
        }


        // =================================================
        // CACHE
        // =================================================

        cachedData =
            data;


        // =================================================
        // WRITE JSON
        // =================================================

        try
        {
            string json =
                JsonUtility.ToJson(
                    data,
                    true
                );


            File.WriteAllText(
                savePath,
                json
            );


            Debug.Log(
                $"<color=green>" +
                $"[BuildingSaveManager] WORLD SAVED" +
                $"</color> | " +
                $"Structures: " +
                $"{data.buildingStructures.Count} | " +
                $"Base Tiles: " +
                $"{data.baseTiles.Count}"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[BuildingSaveManager] Save failed: " +
                e.Message
            );
        }
    }


    // =====================================================
    // SAVE BUILDING STRUCTURES
    // =====================================================

    private void SaveBuildingStructures(
        SaveData data)
    {
        data.buildingStructures.Clear();


        if (
            baseTilePlacementManager == null)
        {
            baseTilePlacementManager =
                BaseTilePlacementManager.Instance;
        }


        if (baseTilePlacementManager == null)
        {
            Debug.LogWarning(
                "[BuildingSaveManager] " +
                "BaseTilePlacementManager missing. " +
                "Structures were not saved."
            );

            return;
        }


        if (
            baseTilePlacementManager.structures == null)
        {
            return;
        }


        foreach (
            BuildingStructure structure
            in baseTilePlacementManager.structures)
        {
            if (
                structure == null ||
                structure.itemType == null ||
                structure.cells == null ||
                structure.cells.Count == 0)
            {
                continue;
            }


            BuildingStructureSaveData save =
                new BuildingStructureSaveData();


            // =================================================
            // ID
            // =================================================

            save.id =
                structure.id;


            // =================================================
            // ITEM
            // =================================================

            save.itemID =
                structure.itemType.itemID;


            // =================================================
            // CELLS
            // =================================================

            save.cells =
                new List<Vector3Int>(
                    structure.cells
                );


            // =================================================
            // ROOM
            // =================================================

            save.isEnclosedRoom =
                structure.isEnclosedRoom;


            save.totalWallCount =
                structure.totalWallCount;


            save.totalDoorCount =
                structure.totalDoorCount;


            data.buildingStructures.Add(
                save
            );
        }


        Debug.Log(
            $"<color=cyan>" +
            $"[ROOM SYSTEM] SAVED STRUCTURES: " +
            $"{data.buildingStructures.Count}" +
            $"</color>"
        );
    }


    // =====================================================
    // SAVE BASE TILES
    // =====================================================

    private void SaveBaseTiles(
        SaveData data)
    {
        data.baseTiles.Clear();


        if (tileSaver == null)
        {
            Debug.LogWarning(
                "[BuildingSaveManager] TileSaver missing."
            );

            return;
        }


        if (tileSaver.tilemaps == null)
        {
            Debug.LogWarning(
                "[BuildingSaveManager] No tilemaps assigned."
            );

            return;
        }


        if (database == null)
        {
            Debug.LogError(
                "[BuildingSaveManager] Database missing."
            );

            return;
        }


        for (
            int mapIndex = 0;
            mapIndex < tileSaver.tilemaps.Length;
            mapIndex++)
        {
            Tilemap tilemap =
                tileSaver.tilemaps[mapIndex];


            if (tilemap == null)
                continue;


            BoundsInt bounds =
                tilemap.cellBounds;


            foreach (
                Vector3Int cell
                in bounds.allPositionsWithin)
            {
                TileBase tile =
                    tilemap.GetTile(cell);


                if (tile == null)
                    continue;


                buildSO build =
                    FindBuildItemForTile(
                        tile
                    );


                if (build == null)
                {
                    Debug.LogWarning(
                        "[BuildingSaveManager] " +
                        "Could not identify tile: " +
                        tile.name
                    );

                    continue;
                }


                data.baseTiles.Add(
                    new BaseTileData
                    {
                        tilemapIndex =
                            mapIndex,

                        x =
                            cell.x,

                        y =
                            cell.y,

                        tileID =
                            build.itemID
                    }
                );
            }
        }
    }


    // =====================================================
    // FIND BUILD ITEM FOR TILE
    // =====================================================

    private buildSO FindBuildItemForTile(
        TileBase targetTile)
    {
        if (targetTile == null)
            return null;


        if (database == null)
            return null;


        buildSO result =
            FindBuildItemInList(
                database.Tools,
                targetTile
            );


        if (result != null)
            return result;


        result =
            FindBuildItemInList(
                database.Placeables,
                targetTile
            );


        if (result != null)
            return result;


        result =
            FindBuildItemInList(
                database.Resources,
                targetTile
            );


        if (result != null)
            return result;


        return null;
    }


    // =====================================================
    // FIND BUILD ITEM IN LIST
    // =====================================================

    private buildSO FindBuildItemInList(
        IEnumerable<ItemData> items,
        TileBase targetTile)
    {
        if (items == null)
            return null;


        foreach (
            ItemData item
            in items)
        {
            if (item is not buildSO build)
                continue;


            if (!build.baseTile)
                continue;


            if (build.placeablePrefab == null)
                continue;


            Tilemap prefabTilemap =
                build.placeablePrefab
                    .GetComponentInChildren<Tilemap>(
                        true
                    );


            if (prefabTilemap == null)
                continue;


            BoundsInt bounds =
                prefabTilemap.cellBounds;


            foreach (
                Vector3Int cell
                in bounds.allPositionsWithin)
            {
                TileBase prefabTile =
                    prefabTilemap.GetTile(cell);


                if (prefabTile == null)
                    continue;


                if (prefabTile == targetTile)
                {
                    return build;
                }
            }
        }


        return null;
    }


    // =====================================================
    // LOAD EVERYTHING
    // =====================================================

    public void LoadEverything()
    {
        if (!File.Exists(savePath))
        {
            HasLoadedWorld = true;

            OnWorldLoaded?.Invoke();

            return;
        }


        try
        {
            string json =
                File.ReadAllText(
                    savePath
                );


            cachedData =
                JsonUtility.FromJson<SaveData>(
                    json
                );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[BuildingSaveManager] Load failed: " +
                e.Message
            );


            cachedData =
                new SaveData();
        }


        if (cachedData == null)
        {
            cachedData =
                new SaveData();
        }


        // =================================================
        // NULL SAFETY
        // =================================================

        if (cachedData.buildings == null)
            cachedData.buildings =
                new List<BuildingData>();


        if (cachedData.baseTiles == null)
            cachedData.baseTiles =
                new List<BaseTileData>();


        if (cachedData.buildingStructures == null)
            cachedData.buildingStructures =
                new List<BuildingStructureSaveData>();


        if (cachedData.plants == null)
            cachedData.plants =
                new List<PlantSaveData>();


        if (cachedData.npcInventories == null)
            cachedData.npcInventories =
                new List<NpcInventorySaveData>();


        if (cachedData.bushes == null)
            cachedData.bushes =
                new List<BushSaveData>();


        if (cachedData.leafdata == null)
            cachedData.leafdata =
                new List<LeafData>();


        if (cachedData.stonedata == null)
            cachedData.stonedata =
                new List<stonedata>();


        if (cachedData.trees == null)
            cachedData.trees =
                new List<TreeSaveData>();


        if (cachedData.soilTiles == null)
            cachedData.soilTiles =
                new List<SoilData>();


        // =================================================
        // PLAYER
        // =================================================

        if (player != null)
        {
            CharacterController cc =
                player.GetComponent<CharacterController>();


            if (cc != null)
                cc.enabled = false;


            player.position =
                cachedData.playerPosition;


            PlayerNeeds needs =
                player.GetComponent<PlayerNeeds>();


            if (needs != null)
            {
                needs.saturation =
                    cachedData.saturation;
            }


            if (cc != null)
                cc.enabled = true;
        }


        // =================================================
        // BUILDINGS
        // =================================================

        foreach (
            BuildingData b
            in cachedData.buildings)
        {
            ItemData item =
                database.GetItemByID(
                    b.itemID
                );


            if (
                item == null ||
                item is not buildSO build)
            {
                continue;
            }


            if (build.baseTile)
                continue;


            if (build.placeablePrefab == null)
                continue;


            GameObject obj =
                Instantiate(
                    build.placeablePrefab,
                    b.position,
                    Quaternion.identity
                );


            if (obj.TryGetComponent(
                out ISaveableBuilding saveable))
            {
                saveable.LoadSaveData(
                    b.currentAmmo,
                    b.plantProgress,
                    b.structuralDurability
                );
            }


            // =================================================
            // CAMPFIRE
            // =================================================

            if (obj.TryGetComponent(
                out CampfireBehav campfire))
            {
                campfire.fuelAmount =
                    b.fuelAmount;


                campfire.isBurning =
                    b.isBurning;


                campfire.validFuelItems =
                    new List<ItemData>();


                if (
                    b.fuelItemIDs != null &&
                    b.fuelItemIDs.Count > 0)
                {
                    foreach (
                        int id
                        in b.fuelItemIDs)
                    {
                        ItemData fuel =
                            database.GetItemByID(id);


                        if (fuel != null)
                        {
                            campfire.validFuelItems.Add(
                                fuel
                            );
                        }
                    }
                }
                else if (
                    b.fuelItemID != -1)
                {
                    ItemData fuel =
                        database.GetItemByID(
                            b.fuelItemID
                        );


                    if (fuel != null)
                    {
                        campfire.validFuelItems.Add(
                            fuel
                        );
                    }
                }
            }


            RegisterBuilding(
                obj
            );
        }


        // =================================================
        // BASE TILEMAPS
        //
        // MUST HAPPEN BEFORE STRUCTURES.
        // =================================================

        LoadBaseTiles();


        // =================================================
        // RESTORE STRUCTURES
        // =================================================

        LoadBuildingStructures();


        // =================================================
        // PLANTS
        // =================================================

        foreach (
            PlantSaveData p
            in cachedData.plants)
        {
            GameObject prefab =
                Resources
                    .FindObjectsOfTypeAll<GameObject>()
                    .FirstOrDefault(
                        x =>
                            x.name ==
                            p.plantPrefabName
                    );


            if (prefab == null)
            {
                Debug.LogWarning(
                    "[BuildingSaveManager] " +
                    "Missing plant prefab: " +
                    p.plantPrefabName
                );

                continue;
            }


            GameObject plantObj =
                Instantiate(
                    prefab,
                    p.position,
                    Quaternion.identity
                );


            if (plantObj.TryGetComponent(
                out PlantBehav plant))
            {
                plant.LoadPlantSaveData(p);
            }
        }


        // =================================================
        // NPC INVENTORIES
        // =================================================

        foreach (
            NpcInvBrain npc
            in FindObjectsByType<NpcInvBrain>(
                FindObjectsSortMode.None))
        {
            if (npc == null)
                continue;


            NpcInventorySaveData save =
                cachedData.npcInventories.Find(
                    x =>
                        x.npcId ==
                        npc.npcId
                );


            if (save != null)
            {
                npc.LoadFromSave(
                    save,
                    database
                );
            }
        }


        // =================================================
        // BUSHES
        // =================================================

        foreach (
            BushSaveData s
            in cachedData.bushes)
        {
            BushBehav existing =
                FindObjectsByType<BushBehav>(
                    FindObjectsSortMode.None
                )
                .FirstOrDefault(
                    bu =>
                        bu.UniqOverworldItemID ==
                        s.uniqID
                );


            if (existing != null)
            {
                existing.LoadData(s);
            }
            else if (bushPrefab != null)
            {
                GameObject newBush =
                    Instantiate(
                        bushPrefab,
                        s.position,
                        Quaternion.identity
                    );


                if (newBush.TryGetComponent(
                    out BushBehav bush))
                {
                    bush.LoadData(s);
                }
            }
        }


        // =================================================
        // LEAVES
        // =================================================

        foreach (
            LeafData l
            in cachedData.leafdata)
        {
            if (l.destroyed)
                continue;


            if (leafPrefab == null)
                continue;


            GameObject leafObj =
                Instantiate(
                    leafPrefab,
                    l.position,
                    Quaternion.identity
                );


            if (leafObj.TryGetComponent(
                out leafbehav leaf))
            {
                leaf.LoadData(l);
            }
        }


        // =================================================
        // STONES
        // =================================================

        foreach (
            stonedata s
            in cachedData.stonedata)
        {
            if (s.destroyed)
                continue;


            if (stonePrefab == null)
                continue;


            GameObject stoneObj =
                Instantiate(
                    stonePrefab,
                    s.position,
                    Quaternion.identity
                );


            if (stoneObj.TryGetComponent(
                out stoneWORLDobjectbehav stone))
            {
                stone.LoadData(s);
            }
        }


        // =================================================
        // TREES
        // =================================================

        HashSet<string> spawnedTreeIDs =
            new HashSet<string>();


        foreach (
            TreeSaveData t
            in cachedData.trees)
        {
            if (!spawnedTreeIDs.Add(
                t.treeID.ToString()))
            {
                continue;
            }


            if (t.isCut)
            {
                if (stumpPrefab == null)
                    continue;


                GameObject stump =
                    Instantiate(
                        stumpPrefab,
                        t.position,
                        Quaternion.identity
                    );


                if (stump.TryGetComponent(
                    out TreeStumpRegrow regrow))
                {
                    regrow.Setup(
                        t.treeID,
                        t.cutTime
                    );
                }
            }
            else
            {
                if (treePrefab == null)
                    continue;


                GameObject treeObj =
                    Instantiate(
                        treePrefab,
                        t.position,
                        Quaternion.identity
                    );


                if (treeObj.TryGetComponent(
                    out treeItemBehav tree))
                {
                    tree.LoadData(t);

                    tree.OnSpawnFromWorld();
                }
            }
        }


        // =================================================
        // SOIL
        // =================================================

        if (
            tiltManager != null &&
            cachedData.soilTiles != null)
        {
            tiltManager.LoadSoilTiles(
                cachedData.soilTiles
            );
        }


        // =================================================
        // TIME
        // =================================================

        DayNightCycle time =
            FindFirstObjectByType<DayNightCycle>();


        if (time != null)
        {
            time.LoadTime(
                cachedData.totalTime,
                cachedData.rawTime,
                cachedData.daysPassed
            );
        }


        // =================================================
        // COMPLETE
        // =================================================

        HasLoadedWorld = true;


        Debug.Log(
            $"<color=green>" +
            $"[BuildingSaveManager] WORLD LOADED" +
            $"</color> | " +
            $"Structures: " +
            $"{cachedData.buildingStructures.Count}"
        );


        OnWorldLoaded?.Invoke();
    }


    // =====================================================
    // LOAD BUILDING STRUCTURES
    // =====================================================

    private void LoadBuildingStructures()
    {
        if (
            baseTilePlacementManager == null)
        {
            baseTilePlacementManager =
                BaseTilePlacementManager.Instance;
        }


        if (baseTilePlacementManager == null)
        {
            Debug.LogWarning(
                "[BuildingSaveManager] " +
                "BaseTilePlacementManager missing. " +
                "Structures could not be loaded."
            );

            return;
        }


        baseTilePlacementManager
            .RestoreSavedStructures(
                cachedData.buildingStructures,
                database
            );
    }


    // =====================================================
    // LOAD BASE TILEMAPS
    // =====================================================

    private void LoadBaseTiles()
    {
        if (tileSaver == null)
        {
            Debug.LogWarning(
                "[BuildingSaveManager] TileSaver missing."
            );

            return;
        }


        if (tileSaver.tilemaps == null)
        {
            Debug.LogWarning(
                "[BuildingSaveManager] No tilemaps assigned."
            );

            return;
        }


        if (database == null)
        {
            Debug.LogError(
                "[BuildingSaveManager] Database missing."
            );

            return;
        }


        if (
            cachedData == null ||
            cachedData.baseTiles == null)
        {
            return;
        }


        int loadedCount = 0;


        // =================================================
        // LOOP SAVED TILES
        // =================================================

        foreach (
            BaseTileData savedTile
            in cachedData.baseTiles)
        {
            if (
                savedTile.tilemapIndex < 0 ||
                savedTile.tilemapIndex >=
                    tileSaver.tilemaps.Length)
            {
                continue;
            }


            Tilemap targetTilemap =
                tileSaver.tilemaps[
                    savedTile.tilemapIndex
                ];


            if (targetTilemap == null)
                continue;


            ItemData item =
                database.GetItemByID(
                    savedTile.tileID
                );


            if (
                item == null ||
                item is not buildSO build)
            {
                continue;
            }


            if (!build.baseTile)
                continue;


            if (build.placeablePrefab == null)
                continue;


            Tilemap sourceTilemap =
                build.placeablePrefab
                    .GetComponentInChildren<Tilemap>(
                        true
                    );


            if (sourceTilemap == null)
            {
                Debug.LogWarning(
                    "[BuildingSaveManager] " +
                    "No Tilemap on base tile prefab: " +
                    build.name
                );

                continue;
            }


            TileBase sourceTile = null;


            BoundsInt sourceBounds =
                sourceTilemap.cellBounds;


            foreach (
                Vector3Int sourceCell
                in sourceBounds.allPositionsWithin)
            {
                TileBase tile =
                    sourceTilemap.GetTile(
                        sourceCell
                    );


                if (tile == null)
                    continue;


                sourceTile =
                    tile;


                break;
            }


            if (sourceTile == null)
                continue;


            Vector3Int targetCell =
                new Vector3Int(
                    savedTile.x,
                    savedTile.y,
                    0
                );


            targetTilemap.SetTile(
                targetCell,
                sourceTile
            );


            loadedCount++;
        }


        // =================================================
        // REFRESH
        // =================================================

        for (
            int i = 0;
            i < tileSaver.tilemaps.Length;
            i++)
        {
            Tilemap tilemap =
                tileSaver.tilemaps[i];


            if (tilemap == null)
                continue;


            tilemap.RefreshAllTiles();


            CompositeCollider2D collider =
                tilemap.GetComponent<
                    CompositeCollider2D
                >();


            if (collider != null)
            {
                collider.GenerateGeometry();
            }
        }


        Debug.Log(
            $"<color=cyan>" +
            $"[BuildingSaveManager] " +
            $"Loaded base tiles: {loadedCount}" +
            $"</color>"
        );
    }


    // =====================================================
    // SAVE AFTER CHANGE
    // =====================================================

    public void SaveAfterChange()
    {
        SaveNow();
    }


    // =====================================================
    // GET SAVE DATA
    // =====================================================

    public SaveData GetSaveData()
    {
        return cachedData;
    }
}