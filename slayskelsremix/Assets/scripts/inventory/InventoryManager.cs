using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;
        public InventorySlot(ItemData d, int c) { item = d; count = c; }
    }

    [Header("Inventory Data")]
    public List<InventorySlot> inventory = new List<InventorySlot>();

    [Header("Dependencies")]
    [Tooltip("Drag your ItemDatabase ScriptableObject here!")]
    public ItemDatabase database;

    private string savePath;

    private void Awake()
    {
        // Path varies by OS (Windows: AppData/LocalLow, etc.)
        savePath = Application.persistentDataPath + "/inventory.json";

        // Auto-load when the game starts
        LoadInventory();
    }

    public void AddItem(ItemData data, int amount)
    {
        if (data == null)
        {
            Debug.LogError("[Inv] Cannot add null ItemData!");
            return;
        }

        bool itemFound = false;

        // Check if we already have this item
        foreach (var slot in inventory)
        {
            if (slot.item == data)
            {
                slot.count += amount;
                itemFound = true;
                Debug.Log($"<color=green>[Inv]</color> Added {amount} to {data.itemName}. Total: {slot.count}");
                break;
            }
        }

        // If it's a new item, add a new slot
        if (!itemFound)
        {
            inventory.Add(new InventorySlot(data, amount));
            Debug.Log($"<color=cyan>[Inv]</color> Added NEW item: {data.itemName}");
        }

        // Auto-save every time an item is picked up
        SaveInventory();
    }

    [ContextMenu("Save Inventory")]
    public void SaveInventory()
    {
        InventorySaveData dataToSave = new InventorySaveData();

        foreach (var slot in inventory)
        {
            if (slot.item != null)
            {
                dataToSave.savedItems.Add(new SaveSlot { itemId = slot.item.itemID, count = slot.count });
            }
        }

        string json = JsonUtility.ToJson(dataToSave, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"<color=green>[System]</color> Inventory saved to: {savePath}");
    }

    [ContextMenu("Load Inventory")]
    public void LoadInventory()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("<color=orange>[System]</color> No save file found. Starting fresh.");
            return;
        }

        if (database == null)
        {
            Debug.LogError("[System] Load Failed! ItemDatabase is not assigned in the Inspector.");
            return;
        }

        string json = File.ReadAllText(savePath);
        InventorySaveData loadedData = JsonUtility.FromJson<InventorySaveData>(json);

        inventory.Clear();
        foreach (var savedSlot in loadedData.savedItems)
        {
            // Find the ScriptableObject reference using the ID from the file
            ItemData item = database.GetItemByID(savedSlot.itemId);
            if (item != null)
            {
                inventory.Add(new InventorySlot(item, savedSlot.count));
            }
            else
            {
                Debug.LogWarning($"[System] Item ID {savedSlot.itemId} found in save but not in Database!");
            }
        }
        Debug.Log("<color=cyan>[System]</color> Inventory loaded from file.");
    }

    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            inventory.Clear();
            Debug.Log("<color=red>[System]</color> Save file deleted and inventory wiped.");
        }
    }
}

// --- SERIALIZATION CLASSES ---
// These allow Unity to convert your list into a JSON file

[System.Serializable]
public class SaveSlot
{
    public int itemId;
    public int count;
}

[System.Serializable]
public class InventorySaveData
{
    public List<SaveSlot> savedItems = new List<SaveSlot>();
}