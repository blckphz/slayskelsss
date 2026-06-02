using System.Collections.Generic;
using UnityEngine;
using static BuildingSaveManager;

[System.Serializable]
public class SaveData
{
    public List<BuildingData> buildings = new();
    public List<NpcInventorySaveData> npcInventories = new();
    public List<BushSaveData> bushes = new();
    public List<TreeSaveData> trees = new();

    public List<SoilData> soilTiles = new();
}