using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
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


        Debug.Log(
            "[BuildingSaveManager] Ready."
        );


        Debug.Log(
            "[BuildingSaveManager] Save path: " +
            savePath
        );
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
        Debug.Log(
            "[BuildingSaveManager] ======================="
        );


        Debug.Log(
            "[BuildingSaveManager] SAVE START"
        );


        SaveData data =
            new SaveData();


        // =================================================
        // PLAYER
        // =================================================

        if (player != null)
        {
            data.playerPosition =
                player.position;
        }


        placedObjects.RemoveAll(
            x => x == null
        );


        if (player != null)
        {
            PlayerNeeds needs =
                player.GetComponent<PlayerNeeds>();


            if (needs != null)
            {
                data.saturation =
                    needs.saturation;
            }
        }


        // =================================================
        // BUILDINGS
        // =================================================

        foreach (
            GameObject obj
            in placedObjects)
        {
            if (
                !obj.TryGetComponent(
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


            // =============================================
            // CAMPFIRE
            // =============================================

            if (
                obj.TryGetComponent(
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
                        var fuel
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

        var plants =
            FindObjectsByType<PlantBehav>(
                FindObjectsSortMode.None
            );


        foreach (
            var plant
            in plants)
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
            var t =
                tree.GetSaveData();


            if (
                !seenTreeIDs.Add(
                    t.treeID.ToString()))
            {
                continue;
            }


            data.trees.Add(t);
        }


        foreach (
            TreeStumpRegrow stump
            in FindObjectsByType<
                TreeStumpRegrow>(
                FindObjectsSortMode.None))
        {
            if (
                !seenTreeIDs.Add(
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
        // TIME
        // =================================================

        var time =
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
        // JSON
        // =================================================

        string json =
            JsonUtility.ToJson(
                data,
                true
            );


        // =================================================
        // WRITE FILE
        // =================================================

        try
        {
            File.WriteAllText(
                savePath,
                json
            );


            Debug.Log(
                "[BuildingSaveManager] " +
                "SAVE FILE WRITTEN"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[BuildingSaveManager] " +
                "FAILED TO WRITE SAVE FILE\n" +
                e
            );


            return;
        }


        // =================================================
        // SUMMARY
        // =================================================

        Debug.Log(
            "[BuildingSaveManager] " +
            "Buildings saved: " +
            data.buildings.Count
        );


        Debug.Log(
            "[BuildingSaveManager] " +
            "Base tiles saved: " +
            data.baseTiles.Count
        );


        Debug.Log(
            "[BuildingSaveManager] " +
            "File: " +
            savePath
        );


        Debug.Log(
            "[BuildingSaveManager] SAVE COMPLETE"
        );


        Debug.Log(
            "[BuildingSaveManager] ======================="
        );
    }


    // =====================================================
    // SAVE BASE TILES
    // =====================================================

    private void SaveBaseTiles(
        SaveData data)
    {
        data.baseTiles.Clear();


        Debug.Log(
            "[BuildingSaveManager] " +
            "SAVE BASE TILEMAPS START"
        );


        if (tileSaver == null)
        {
            Debug.LogError(
                "[BuildingSaveManager] " +
                "TileSaver is NULL!"
            );


            return;
        }


        if (tileSaver.tilemaps == null)
        {
            Debug.LogError(
                "[BuildingSaveManager] " +
                "TileSaver.tilemaps is NULL!"
            );


            return;
        }


        if (database == null)
        {
            Debug.LogError(
                "[BuildingSaveManager] " +
                "Database is NULL!"
            );


            return;
        }


        Debug.Log(
            "[BuildingSaveManager] " +
            "Tilemaps found: " +
            tileSaver.tilemaps.Length
        );


        // =================================================
        // LOOP ALL TILEMAPS
        // =================================================

        for (
            int mapIndex = 0;
            mapIndex < tileSaver.tilemaps.Length;
            mapIndex++)
        {
            Tilemap tilemap =
                tileSaver.tilemaps[mapIndex];


            if (tilemap == null)
            {
                Debug.LogError(
                    "[BuildingSaveManager] " +
                    "Tilemap " +
                    mapIndex +
                    " is NULL!"
                );


                continue;
            }


            BoundsInt bounds =
                tilemap.cellBounds;


            Debug.Log(
                "[BuildingSaveManager] " +
                "Checking Tilemap " +
                mapIndex +
                " | " +
                tilemap.name +
                " | Bounds=" +
                bounds
            );


            int mapTileCount = 0;


            foreach (
                Vector3Int cell
                in bounds.allPositionsWithin)
            {
                TileBase tile =
                    tilemap.GetTile(cell);


                if (tile == null)
                    continue;


                mapTileCount++;


                Debug.Log(
                    "[BuildingSaveManager] " +
                    "FOUND TILE | Map=" +
                    mapIndex +
                    " | MapName=" +
                    tilemap.name +
                    " | Cell=" +
                    cell +
                    " | Tile=" +
                    tile.name
                );


                // =========================================
                // FIND BUILD ITEM
                // =========================================

                buildSO build =
                    FindBuildItemForTile(
                        tile
                    );


                if (build == null)
                {
                    Debug.LogWarning(
                        "[BuildingSaveManager] " +
                        "FOUND TILE BUT NO buildSO | " +
                        "Map=" +
                        mapIndex +
                        " | Cell=" +
                        cell +
                        " | Tile=" +
                        tile.name
                    );


                    continue;
                }


                if (!build.baseTile)
                {
                    Debug.LogWarning(
                        "[BuildingSaveManager] " +
                        "Tile matched buildSO but " +
                        "baseTile=false | " +
                        build.name
                    );


                    continue;
                }


                // =========================================
                // SAVE
                // =========================================

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


                Debug.Log(
                    "[BuildingSaveManager] " +
                    "SAVED TILE | " +
                    "Map=" +
                    mapIndex +
                    " | Cell=" +
                    cell +
                    " | Item=" +
                    build.name +
                    " | ItemID=" +
                    build.itemID
                );
            }


            Debug.Log(
                "[BuildingSaveManager] " +
                "Tilemap " +
                mapIndex +
                " contains " +
                mapTileCount +
                " actual tiles."
            );
        }


        Debug.Log(
            "[BuildingSaveManager] " +
            "TOTAL BASE TILES SAVED = " +
            data.baseTiles.Count
        );


        Debug.Log(
            "[BuildingSaveManager] " +
            "SAVE BASE TILEMAPS END"
        );
    }


    // =====================================================
    // FIND BUILD ITEM FOR TILE
    // =====================================================

    private buildSO FindBuildItemForTile(
        TileBase tile)
    {
        if (tile == null)
            return null;


        if (database == null)
            return null;


        // =================================================
        // TOOLS
        // =================================================

        if (database.Tools != null)
        {
            foreach (
                ItemData item
                in database.Tools)
            {
                if (
                    item is not buildSO build)
                {
                    continue;
                }


                if (!build.baseTile)
                    continue;


                if (
                    build.baseTileAsset ==
                    tile)
                {
                    return build;
                }
            }
        }


        // =================================================
        // PLACEABLES
        // =================================================

        if (database.Placeables != null)
        {
            foreach (
                ItemData item
                in database.Placeables)
            {
                if (
                    item is not buildSO build)
                {
                    continue;
                }


                if (!build.baseTile)
                    continue;


                if (
                    build.baseTileAsset ==
                    tile)
                {
                    return build;
                }
            }
        }


        // =================================================
        // RESOURCES
        // =================================================

        if (database.Resources != null)
        {
            foreach (
                ItemData item
                in database.Resources)
            {
                if (
                    item is not buildSO build)
                {
                    continue;
                }


                if (!build.baseTile)
                    continue;


                if (
                    build.baseTileAsset ==
                    tile)
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
        Debug.Log(
            "[BuildingSaveManager] " +
            "LOAD START"
        );


        if (!File.Exists(savePath))
        {
            Debug.Log(
                "[BuildingSaveManager] " +
                "No save file exists."
            );


            HasLoadedWorld =
                true;


            OnWorldLoaded?.Invoke();


            return;
        }


        string json =
            File.ReadAllText(
                savePath
            );


        cachedData =
            JsonUtility.FromJson<SaveData>(
                json
            );


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
                player.GetComponent<
                    CharacterController
                >();


            if (cc != null)
                cc.enabled = false;


            player.position =
                cachedData.playerPosition;


            PlayerNeeds needs =
                player.GetComponent<
                    PlayerNeeds
                >();


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
            var b
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


            // Base tiles are loaded separately.
            if (build.baseTile)
                continue;


            GameObject obj =
                Instantiate(
                    build.placeablePrefab,
                    b.position,
                    Quaternion.identity
                );


            if (
                obj.TryGetComponent(
                    out ISaveableBuilding saveable))
            {
                saveable.LoadSaveData(
                    b.currentAmmo,
                    b.plantProgress,
                    b.structuralDurability
                );
            }


            // =============================================
            // CAMPFIRE
            // =============================================

            if (
                obj.TryGetComponent(
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
                            database.GetItemByID(
                                id
                            );


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


            RegisterBuilding(obj);
        }


        // =================================================
        // BASE TILEMAPS
        // =================================================

        LoadBaseTiles();


        // =================================================
        // PLANTS
        // =================================================

        foreach (
            var p
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
                Debug.LogError(
                    "[PLANT LOAD] " +
                    "Missing prefab: " +
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


            if (
                plantObj.TryGetComponent(
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
            in FindObjectsByType<
                NpcInvBrain>(
                FindObjectsSortMode.None))
        {
            var save =
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
                FindObjectsByType<
                    BushBehav>(
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


                if (
                    newBush.TryGetComponent(
                        out BushBehav b))
                {
                    b.LoadData(s);
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


            GameObject leafObj =
                Instantiate(
                    leafPrefab,
                    l.position,
                    Quaternion.identity
                );


            if (
                leafObj.TryGetComponent(
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


            GameObject stoneObj =
                Instantiate(
                    stonePrefab,
                    s.position,
                    Quaternion.identity
                );


            if (
                stoneObj.TryGetComponent(
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
            if (
                !spawnedTreeIDs.Add(
                    t.treeID.ToString()))
            {
                continue;
            }


            if (t.isCut)
            {
                GameObject stump =
                    Instantiate(
                        stumpPrefab,
                        t.position,
                        Quaternion.identity
                    );


                if (
                    stump.TryGetComponent(
                        out TreeStumpRegrow r))
                {
                    r.Setup(
                        t.treeID,
                        t.cutTime
                    );
                }
            }
            else
            {
                GameObject treeObj =
                    Instantiate(
                        treePrefab,
                        t.position,
                        Quaternion.identity
                    );


                if (
                    treeObj.TryGetComponent(
                        out treeItemBehav tr))
                {
                    tr.LoadData(t);

                    tr.OnSpawnFromWorld();
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

        var time =
            FindFirstObjectByType<
                DayNightCycle
            >();


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

        HasLoadedWorld =
            true;


        Debug.Log(
            "[BuildingSaveManager] " +
            "LOAD COMPLETE | " +
            "Base tiles loaded = " +
            cachedData.baseTiles.Count
        );


        OnWorldLoaded?.Invoke();
    }


    // =====================================================
    // LOAD BASE TILEMAPS
    // =====================================================

    private void LoadBaseTiles()
    {
        Debug.Log(
            "[BuildingSaveManager] " +
            "LOAD BASE TILEMAPS START"
        );


        if (tileSaver == null)
        {
            Debug.LogError(
                "[BuildingSaveManager] " +
                "TileSaver is NULL!"
            );


            return;
        }


        if (tileSaver.tilemaps == null)
        {
            Debug.LogError(
                "[BuildingSaveManager] " +
                "TileSaver.tilemaps is NULL!"
            );


            return;
        }


        if (database == null)
        {
            Debug.LogError(
                "[BuildingSaveManager] " +
                "Database is NULL!"
            );


            return;
        }


        if (
            cachedData == null ||
            cachedData.baseTiles == null)
        {
            Debug.LogWarning(
                "[BuildingSaveManager] " +
                "No base tile save data."
            );


            return;
        }


        int loadedCount = 0;


        foreach (
            BaseTileData savedTile
            in cachedData.baseTiles)
        {
            // =============================================
            // MAP INDEX
            // =============================================

            if (
                savedTile.tilemapIndex < 0 ||
                savedTile.tilemapIndex >=
                    tileSaver.tilemaps.Length)
            {
                Debug.LogError(
                    "[BuildingSaveManager] " +
                    "Invalid tilemap index: " +
                    savedTile.tilemapIndex
                );


                continue;
            }


            Tilemap tilemap =
                tileSaver.tilemaps[
                    savedTile.tilemapIndex
                ];


            if (tilemap == null)
            {
                Debug.LogError(
                    "[BuildingSaveManager] " +
                    "Tilemap is NULL at index " +
                    savedTile.tilemapIndex
                );


                continue;
            }


            // =============================================
            // ITEM
            // =============================================

            ItemData item =
                database.GetItemByID(
                    savedTile.tileID
                );


            if (
                item == null ||
                item is not buildSO build)
            {
                Debug.LogError(
                    "[BuildingSaveManager] " +
                    "Cannot find buildSO for ItemID=" +
                    savedTile.tileID
                );


                continue;
            }


            if (!build.baseTile)
            {
                Debug.LogError(
                    "[BuildingSaveManager] " +
                    "Saved item is not a base tile: " +
                    build.name
                );


                continue;
            }


            if (build.baseTileAsset == null)
            {
                Debug.LogError(
                    "[BuildingSaveManager] " +
                    "Base tile asset is NULL: " +
                    build.name
                );


                continue;
            }


            // =============================================
            // CELL
            // =============================================

            Vector3Int cell =
                new Vector3Int(
                    savedTile.x,
                    savedTile.y,
                    0
                );


            // =============================================
            // SET
            // =============================================

            tilemap.SetTile(
                cell,
                build.baseTileAsset
            );


            loadedCount++;


            Debug.Log(
                "[BuildingSaveManager] " +
                "LOADED TILE | " +
                "Map=" +
                savedTile.tilemapIndex +
                " | MapName=" +
                tilemap.name +
                " | Cell=" +
                cell +
                " | Item=" +
                build.name +
                " | ItemID=" +
                build.itemID
            );
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
            "[BuildingSaveManager] " +
            "BASE TILEMAP LOAD COMPLETE | " +
            "Tiles loaded=" +
            loadedCount
        );
    }


    // =====================================================
    // SAVE AFTER CHANGE
    // =====================================================

    public void SaveAfterChange()
    {
        Debug.Log(
            "[BuildingSaveManager] " +
            "SaveAfterChange() CALLED"
        );


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