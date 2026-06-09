using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;

    public List<BuildingData> buildings = new();
    public List<NpcInventorySaveData> npcInventories = new();

    public List<BushSaveData> bushes = new();
    public List<TreeSaveData> trees = new();
    public List<SoilData> soilTiles = new();
    public List<LeafData> leafdata = new();
    public List<stonedata> stonedata = new();
}

[System.Serializable]
public class BuildingData
{
    public int itemID;
    public Vector3 position;

    public int currentAmmo;
    public float plantProgress;
    public int structuralDurability;
}