using System.Collections.Generic;
using UnityEngine;


// =====================================================
// SAVE DATA
// =====================================================

[System.Serializable]
public class SaveData
{
    // =========================
    // PLAYER
    // =========================

    public Vector3 playerPosition;

    public int saturation;


    // =========================
    // BUILDINGS
    // =========================

    public List<BuildingData> buildings =
        new List<BuildingData>();


    // =========================
    // PLANTS
    // =========================

    public List<PlantSaveData> plants =
        new List<PlantSaveData>();


    // =========================
    // NPC INVENTORIES
    // =========================

    public List<NpcInventorySaveData> npcInventories =
        new List<NpcInventorySaveData>();


    // =========================
    // BUSHES
    // =========================

    public List<BushSaveData> bushes =
        new List<BushSaveData>();


    // =========================
    // TREES
    // =========================

    public List<TreeSaveData> trees =
        new List<TreeSaveData>();


    // =========================
    // SOIL
    // =========================

    public List<SoilData> soilTiles =
        new List<SoilData>();


    // =========================
    // LEAVES
    // =========================

    public List<LeafData> leafdata =
        new List<LeafData>();


    // =========================
    // STONES
    // =========================

    public List<stonedata> stonedata =
        new List<stonedata>();


    // =========================
    // BASE TILEMAP TILES
    // =========================

    public List<BaseTileData> baseTiles =
        new List<BaseTileData>();


    // =========================
    // TIME
    // =========================

    public float totalTime;

    public float rawTime;

    public int daysPassed;
}


// =====================================================
// BASE TILE DATA
// =====================================================

[System.Serializable]
public class BaseTileData
{
    // Which Tilemap this tile belongs to.
    //
    // 0 = first Tilemap
    // 1 = second Tilemap
    // 2 = third Tilemap
    public int tilemapIndex;


    // Tilemap cell position.
    public int x;

    public int y;


    // buildSO item ID used to identify
    // which tile should be placed.
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


    // =========================
    // CAMPFIRE
    // =========================

    public float fuelAmount;

    public bool isBurning;


    public List<int> fuelItemIDs =
        new List<int>();


    // Old format compatibility.
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