using System.Collections.Generic;
using UnityEngine;


// =====================================================
// SAVE DATA
// =====================================================

[System.Serializable]
public class SaveData
{
    // =================================================
    // PLAYER
    // =================================================

    public Vector3 playerPosition;

    public int saturation;


    // =================================================
    // BUILDINGS
    // =================================================

    public List<BuildingData> buildings =
        new List<BuildingData>();


    // =================================================
    // BASE TILEMAP TILES
    // =================================================

    public List<BaseTileData> baseTiles =
        new List<BaseTileData>();


    // =================================================
    // BUILDING STRUCTURES
    // =================================================

    public List<BuildingStructureSaveData>
        buildingStructures =
        new List<BuildingStructureSaveData>();


    // =================================================
    // PLANTS
    // =================================================

    public List<PlantSaveData> plants =
        new List<PlantSaveData>();


    // =================================================
    // NPC INVENTORIES
    // =================================================

    public List<NpcInventorySaveData> npcInventories =
        new List<NpcInventorySaveData>();


    // =================================================
    // BUSHES
    // =================================================

    public List<BushSaveData> bushes =
        new List<BushSaveData>();


    // =================================================
    // TREES
    // =================================================

    public List<TreeSaveData> trees =
        new List<TreeSaveData>();


    // =================================================
    // SOIL
    // =================================================

    public List<SoilData> soilTiles =
        new List<SoilData>();


    // =================================================
    // LEAVES
    // =================================================

    public List<LeafData> leafdata =
        new List<LeafData>();


    // =================================================
    // STONES
    // =================================================

    public List<stonedata> stonedata =
        new List<stonedata>();


    // =================================================
    // TIME
    // =================================================

    public float totalTime;

    public float rawTime;

    public int daysPassed;
}


// =====================================================
// BUILDING STRUCTURE SAVE DATA
// =====================================================

[System.Serializable]
public class BuildingStructureSaveData
{
    // =================================================
    // STRUCTURE ID
    // =================================================

    public string id;


    // =================================================
    // BUILD ITEM ID
    // =================================================

    public int itemID;


    // =================================================
    // STRUCTURE CELLS
    // =================================================

    public List<Vector3Int> cells =
        new List<Vector3Int>();


    // =================================================
    // ROOM STATUS
    // =================================================

    public bool isEnclosedRoom;


    // =================================================
    // ROOM COUNTERS
    // =================================================

    public int totalWallCount;

    public int totalDoorCount;
}


// =====================================================
// BASE TILE DATA
// =====================================================

[System.Serializable]
public class BaseTileData
{
    public int tilemapIndex;

    public int x;

    public int y;

    public int tileID;
}


// =====================================================
// BUILDING DATA
// =====================================================

[System.Serializable]
public class BuildingData
{
    public int itemID;

    public Vector3 position;


    public int currentAmmo;

    public float plantProgress;

    public int structuralDurability;


    // =================================================
    // CAMPFIRE
    // =================================================

    public float fuelAmount;

    public bool isBurning;


    public List<int> fuelItemIDs =
        new List<int>();


    // Old save compatibility.
    public int fuelItemID = -1;
}


// =====================================================
// PLANT DATA
// =====================================================

[System.Serializable]
public class PlantSaveData
{
    public string plantPrefabName;

    public string plantID;

    public Vector3 position;

    public int growthStage;

    public float growthProgress;

    public int durability;

    public int waterLevel;
}