using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ChestInventory : MonoBehaviour, IInteractable
{
    [Header("Unique ID")]
    public string chestID;

    [Header("Data")]
    public List<ChestSlot> chestItems = new List<ChestSlot>();
    public int maxSlots = 20;

    private string savePath;
    private Highlightable highlight;

    private void Awake()
    {
        highlight = GetComponent<Highlightable>();
        if (string.IsNullOrEmpty(chestID))
            chestID = gameObject.name + "_" + transform.position.GetHashCode();
        savePath = Path.Combine(Application.persistentDataPath, $"chest_{chestID}.json");
        Invoke(nameof(LoadChest), 0.1f);
    }

    // ==========================================================
    // 1. FIX FOR InteractionManager (IsEmpty)
    // ==========================================================
    public bool IsEmpty()
    {
        return chestItems == null || chestItems.Count == 0;
    }

    // ==========================================================
    // 2. FIX FOR ChestSlotUI (RemoveItem)
    // ==========================================================
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

    // ==========================================================
    // 3. NPC UTILITY METHODS
    // ==========================================================
    public bool HasItem(int itemID)
    {
        return chestItems.Exists(slot => slot.item != null && slot.item.itemID == itemID && slot.count > 0);
    }

    // Used by NPCBrain to take items by specific index
    public void RemoveItemAtIndex(int index, int amount)
    {
        if (index >= 0 && index < chestItems.Count)
        {
            chestItems[index].count -= amount;
            if (chestItems[index].count <= 0) chestItems.RemoveAt(index);

            SaveChest();
            if (ChestUI.Instance != null && ChestUI.Instance.GetCurrentChest() == this)
                ChestUI.Instance.Refresh();
        }
    }

    // ==========================================================
    // 4. EXISTING INTERFACES & LOGIC
    // ==========================================================
    public void Interact(InventoryManager playerInventory)
    {
        if (ChestUI.Instance != null && ChestUI.Instance.GetCurrentChest() == this) CloseChest();
        else OpenChest();
    }

    public void OnFocus() => highlight?.SetHighlighted(true);
    public void OnLoseFocus() => highlight?.SetHighlighted(false);
    public string GetPrompt() => "Open Chest";
    public void OpenChest() => ChestUI.Instance?.Open(this);
    public void CloseChest() { SaveChest(); ChestUI.Instance?.Close(); }

    public bool AddItem(ItemData item, int amount)
    {
        if (item == null) return false;

        bool found = false;
        foreach (var slot in chestItems)
        {
            if (slot.item == item)
            {
                slot.count += amount;
                found = true;
                break;
            }
        }

        if (!found)
        {
            if (chestItems.Count >= maxSlots) return false;
            chestItems.Add(new ChestSlot(item, amount));
        }

        SaveChest();

        if (ChestUI.Instance != null && ChestUI.Instance.GetCurrentChest() == this)
        {
            ChestUI.Instance.Refresh();
        }

        return true;
    }

    public void SaveChest()
    {
        ChestSaveData data = new ChestSaveData();
        foreach (var slot in chestItems)
        {
            if (slot.item != null)
                data.savedItems.Add(new SaveSlot { itemId = slot.item.itemID, count = slot.count });
        }
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    public void LoadChest()
    {
        if (!File.Exists(savePath)) return;
        if (InventoryManager.Instance == null || InventoryManager.Instance.database == null) return;
        ItemDatabase db = InventoryManager.Instance.database;
        ChestSaveData data = JsonUtility.FromJson<ChestSaveData>(File.ReadAllText(savePath));
        chestItems.Clear();
        foreach (var s in data.savedItems)
        {
            ItemData item = db.GetItemByID(s.itemId);
            if (item != null) chestItems.Add(new ChestSlot(item, s.count));
        }
    }
}