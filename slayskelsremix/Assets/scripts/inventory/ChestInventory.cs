using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ChestInventory : MonoBehaviour
{
    [Header("Unique ID")]
    [Tooltip("IMPORTANT: Give every chest in your scene a unique ID string!")]
    public string chestID;

    [Header("Data")]
    public List<ChestSlot> chestItems = new List<ChestSlot>();
    public int maxSlots = 20;

    private string savePath;

    private void Awake()
    {
        // Use the ID to create a unique filename. 
        // If ID is empty, it falls back to object name + position hash.
        if (string.IsNullOrEmpty(chestID))
            chestID = gameObject.name + "_" + transform.position.GetHashCode();

        savePath = Path.Combine(Application.persistentDataPath, $"chest_{chestID}.json");

        // Wait a frame or ensure InventoryManager is ready before loading
        Invoke(nameof(LoadChest), 0.1f);
    }

    public void OpenChest()
    {
        ChestUI.Instance?.Open(this);
    }

    public void CloseChest()
    {
        SaveChest(); // Final save on close
        ChestUI.Instance?.Close();
    }

    public bool AddItem(ItemData item, int amount)
    {
        if (item == null) return false;

        foreach (var slot in chestItems)
        {
            if (slot.item == item)
            {
                slot.count += amount;
                SaveChest();
                return true;
            }
        }

        if (chestItems.Count >= maxSlots) return false;

        chestItems.Add(new ChestSlot(item, amount));
        SaveChest();
        return true;
    }

    public bool RemoveItem(ItemData item, int amount)
    {
        for (int i = 0; i < chestItems.Count; i++)
        {
            if (chestItems[i].item == item)
            {
                chestItems[i].count -= amount;
                if (chestItems[i].count <= 0)
                {
                    chestItems.RemoveAt(i);
                }
                SaveChest();
                return true;
            }
        }
        return false;
    }

    // ---------------- SAVE / LOAD LOGIC ----------------

    [ContextMenu("Force Save Chest")]
    public void SaveChest()
    {
        ChestSaveData data = new ChestSaveData();

        foreach (var slot in chestItems)
        {
            if (slot.item != null)
            {
                data.savedItems.Add(new SaveSlot
                {
                    itemId = slot.item.itemID,
                    count = slot.count
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"[Chest] {chestID} saved.");
    }

    public void LoadChest()
    {
        if (!File.Exists(savePath)) return;

        // Ensure we have a database to link IDs back to ScriptableObjects
        if (InventoryManager.Instance == null || InventoryManager.Instance.database == null)
        {
            Debug.LogError("[ChestInventory] Database not found! Cannot load items.");
            return;
        }

        ItemDatabase db = InventoryManager.Instance.database;
        string json = File.ReadAllText(savePath);
        ChestSaveData data = JsonUtility.FromJson<ChestSaveData>(json);

        chestItems.Clear();
        foreach (var s in data.savedItems)
        {
            ItemData item = db.GetItemByID(s.itemId);
            if (item != null)
            {
                chestItems.Add(new ChestSlot(item, s.count));
            }
        }
    }
}