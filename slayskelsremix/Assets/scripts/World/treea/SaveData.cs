using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;

    public int saturation; // ✅ ADD THIS
    public List<BuildingData> buildings = new();
    public List<PlantSaveData> plants = new();
    public List<NpcInventorySaveData> npcInventories = new();
    public List<BushSaveData> bushes = new();
    public List<TreeSaveData> trees = new();
    public List<SoilData> soilTiles = new();
    public List<LeafData> leafdata = new();
    public List<stonedata> stonedata = new();

    public float totalTime;
    public float rawTime;
    public int daysPassed;
}



[System.Serializable]
public class BuildingData
{
    public int itemID;
    public Vector3 position;

    public int currentAmmo;
    public float plantProgress;
    public int structuralDurability;

    // =========================
    // CAMPFIRE SAVE DATA
    // =========================
    public float fuelAmount;
    public bool isBurning;

    // MULTI FUEL SUPPORT
    public List<int> fuelItemIDs = new List<int>();

    // OLD FORMAT BACKWARD COMPATIBILITY
    public int fuelItemID = -1;
}

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