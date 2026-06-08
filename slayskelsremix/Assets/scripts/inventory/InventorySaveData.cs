using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveSlot
{
    public int itemId;
    public int count;
    public int currentDurability; // 👈 Upgraded to float to preserve item instance dynamic wear
}

[System.Serializable]
public class HotbarSaveSlot
{
    public int itemId;
    public string abilityName;
    public int count;
    public int currentDurability; // 👈 Upgraded to float to match real-time tool tracking precision
}

[System.Serializable]
public class BuildingSaveSlot
{
    public int itemId;
    public Vector3 position;
    public Quaternion rotation;

    // Core building-specific runtime statistics
    public int ammo;
    public float progress;
    public int currentDurability; // Tracks physical structure health (Kept as int unless structures use floats too)
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
    public List<NpcInventorySaveData> npcInventories = new List<NpcInventorySaveData>();
    public List<BuildingSaveSlot> placedBuildings = new List<BuildingSaveSlot>(); // Tracks world structures
}