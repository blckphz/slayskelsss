using System.Collections.Generic;

[System.Serializable]
public class ChestSaveData
{
    // Reuses the SaveSlot class from your InventoryManager logic
    public List<SaveSlot> savedItems = new List<SaveSlot>();
}