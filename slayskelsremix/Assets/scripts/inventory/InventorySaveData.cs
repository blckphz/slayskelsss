using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveSlot
{
    public int itemId;
    public int count;
}

[System.Serializable]
public class HotbarSaveSlot
{
    public int itemId;
    public string abilityName;
    public int count;
}

[System.Serializable]
public class InventorySaveData
{
    public List<SaveSlot> savedItems = new List<SaveSlot>();
    public List<HotbarSaveSlot> hotbarItems = new List<HotbarSaveSlot>();
}

[System.Serializable]
public class NpcInventorySaveData
{
    public string npcId;
    public List<SaveSlot> items = new List<SaveSlot>();
}

[System.Serializable]
public class WorldSaveData
{
    public List<NpcInventorySaveData> npcInventories =
        new List<NpcInventorySaveData>();
}