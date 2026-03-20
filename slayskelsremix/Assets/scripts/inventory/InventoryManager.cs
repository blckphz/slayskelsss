using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    // 🔥 Added Instance for Singleton access
    public static InventoryManager Instance;

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;

        public InventorySlot(ItemData d, int c)
        {
            item = d;
            count = c;
        }
    }

    [Header("Inventory Data")]
    public List<InventorySlot> inventory = new List<InventorySlot>();

    [Header("Dependencies")]
    public ItemDatabase database;

    private string savePath;

    private void Awake()
    {
        // 🔥 Setup Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Application.persistentDataPath + "/inventory.json";
        LoadInventory();
    }

    public void AddItem(ItemData data, int amount)
    {
        Debug.Log($"[InventoryManager] AddItem: {data?.itemName} x{amount}");

        if (data == null)
        {
            Debug.LogError("[InventoryManager] Tried to add NULL item!");
            return;
        }

        // Check for existing stack
        foreach (var slot in inventory)
        {
            if (slot.item == data)
            {
                slot.count += amount;
                Debug.Log($"[InventoryManager] Stacked {data.itemName}. New count: {slot.count}");
                SaveInventory();

                // Update UI after stacking
                InvUI.Instance?.RefreshUI();
                return;
            }
        }

        // Add as new slot
        inventory.Add(new InventorySlot(data, amount));
        Debug.Log($"[InventoryManager] Added new item: {data.itemName}");

        SaveInventory();

        // Update UI after adding new item
        InvUI.Instance?.RefreshUI();
    }

    public bool RemoveItem(ItemData data, int amount)
    {
        Debug.Log($"[InventoryManager] RemoveItem: {data?.itemName} x{amount}");

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item == data)
            {
                inventory[i].count -= amount;

                Debug.Log($"[InventoryManager] New count: {inventory[i].count}");

                if (inventory[i].count <= 0)
                {
                    Debug.Log($"[InventoryManager] Removing slot for {data.itemName}");
                    inventory.RemoveAt(i);
                }

                SaveInventory();

                // Update UI after removing
                InvUI.Instance?.RefreshUI();
                return true;
            }
        }

        Debug.LogWarning("[InventoryManager] Item not found!");
        return false;
    }

    [ContextMenu("Save Inventory")]
    public void SaveInventory()
    {
        Debug.Log("[InventoryManager] Saving inventory...");

        InventorySaveData dataToSave = new InventorySaveData();

        foreach (var slot in inventory)
        {
            if (slot.item != null)
            {
                dataToSave.savedItems.Add(new SaveSlot
                {
                    itemId = slot.item.itemID,
                    count = slot.count
                });
            }
        }

        string json = JsonUtility.ToJson(dataToSave, true);
        File.WriteAllText(savePath, json);

        Debug.Log("[InventoryManager] Inventory saved.");
    }

    [ContextMenu("Load Inventory")]
    public void LoadInventory()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("[InventoryManager] No save file found.");
            return;
        }

        if (database == null)
        {
            Debug.LogError("[InventoryManager] Database missing!");
            return;
        }

        Debug.Log("[InventoryManager] Loading inventory...");

        string json = File.ReadAllText(savePath);
        InventorySaveData loadedData = JsonUtility.FromJson<InventorySaveData>(json);

        inventory.Clear();

        foreach (var savedSlot in loadedData.savedItems)
        {
            ItemData item = database.GetItemByID(savedSlot.itemId);

            if (item != null)
            {
                inventory.Add(new InventorySlot(item, savedSlot.count));
            }
            else
            {
                Debug.LogWarning($"[InventoryManager] Missing item ID: {savedSlot.itemId}");
            }
        }

        Debug.Log("[InventoryManager] Inventory loaded.");

        // Update UI after loading
        InvUI.Instance?.RefreshUI();
    }
}

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