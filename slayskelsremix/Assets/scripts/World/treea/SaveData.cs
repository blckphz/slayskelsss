using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;

    public List<BuildingSaveManager.BuildingData> buildings =
        new List<BuildingSaveManager.BuildingData>();

    public List<NpcInventorySaveData> npcInventories =
        new List<NpcInventorySaveData>();

    public List<BushSaveData> bushes =
        new List<BushSaveData>();

    public List<TreeSaveData> trees =
        new List<TreeSaveData>();

    public List<SoilData> soilTiles =
        new List<SoilData>();

    public List<ResourceSaveData> resources =
        new List<ResourceSaveData>();

    public List<leafData> leaf =
       new List<leafData>();


}