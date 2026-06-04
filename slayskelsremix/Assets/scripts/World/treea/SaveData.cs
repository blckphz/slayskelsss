using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector2 playerPosition;

    public List<BuildingSaveManager.BuildingData> buildings = new();

    public List<NpcInventorySaveData> npcInventories = new();

    public List<BushSaveData> bushes = new();

    public List<TreeSaveData> trees = new();

    public List<SoilData> soilTiles = new();

    // ALL resources
    public List<ResourceSaveData> stones = new();
}