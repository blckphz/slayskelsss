using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;
        public float currentDurability;

        public InventorySlot(ItemData d, int c, float dur)
        {
            item = d;
            count = c;
            currentDurability = dur;
        }
    }

    [Header("Inventory Data")]
    public List<InventorySlot> inventory = new List<InventorySlot>();
    public List<HotbarSlotData> hotbarData = new List<HotbarSlotData>();
    public int hotbarSize = 9;

    [Header("Dependencies")]
    public ItemDatabase database;
    public AbilityDatabase abilityDatabase;

    private string savePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/inventory.json";

        InitializeHotbarData();
        LoadInventory();
    }

    private void InitializeHotbarData()
    {
        if (hotbarData == null) hotbarData = new List<HotbarSlotData>();

        if (hotbarData.Count != hotbarSize)
        {
            hotbarData.Clear();
            for (int i = 0; i < hotbarSize; i++)
            {
                hotbarData.Add(new HotbarSlotData());
            }
        }
    }

    // ==========================================
    // UPGRADED STACK-CONSCIOUS ADD SYSTEM
    // ==========================================
    public void AddItem(ItemData data, int amount, float customDurability = -1f)
    {
        if (data == null || amount <= 0) return;

        // Items with durability must retain an individual standalone tracking slot
        if (data.maxDurability > 0)
        {
            for (int i = 0; i < amount; i++)
            {
                float appliedDurability = (customDurability < 0) ? data.maxDurability : customDurability;
                inventory.Add(new InventorySlot(data, 1, appliedDurability));
            }
            RefreshAll();
            return;
        }

        // Standard resources stack pooling with respect to maxStackSize constraints
        int remainingAmount = amount;

        // Step 1: Attempt to fill pre-existing stacks that have open room left
        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                int maxStack = data.maxStackSize;
                if (slot.count < maxStack)
                {
                    int spaceLeft = maxStack - slot.count;
                    int addAmount = Mathf.Min(spaceLeft, remainingAmount);

                    slot.count += addAmount;
                    remainingAmount -= addAmount;

                    if (remainingAmount <= 0) break;
                }
            }
        }

        // Step 2: If items remain, assign them to new slots, breaking them up by max stack capacities
        while (remainingAmount > 0)
        {
            int nextStackSize = Mathf.Min(remainingAmount, data.maxStackSize);
            inventory.Add(new InventorySlot(data, nextStackSize, 0f));
            remainingAmount -= nextStackSize;
        }

        RefreshAll();
    }

    public bool RemoveItem(ItemData data, int amount)
    {
        if (data == null) return false;
        bool changed = false;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, amount);
                inventory[i].count -= take;
                amount -= take;
                if (inventory[i].count <= 0) inventory.RemoveAt(i);
                changed = true;
                if (amount <= 0) break;
            }
        }

        if (amount > 0)
        {
            for (int i = 0; i < hotbarData.Count; i++)
            {
                if (hotbarData[i].item != null && hotbarData[i].item.itemID == data.itemID)
                {
                    int take = Mathf.Min(hotbarData[i].count, amount);
                    hotbarData[i].count -= take;
                    amount -= take;
                    if (hotbarData[i].count <= 0) hotbarData[i].Clear();
                    changed = true;
                    if (amount <= 0) break;
                }
            }
        }

        if (changed) RefreshAll();
        return changed;
    }

    public void ConsumeResources(ItemData data, int amount)
    {
        if (data == null) return;
        int remaining = amount;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].item != null && inventory[i].item.itemID == data.itemID)
            {
                int take = Mathf.Min(inventory[i].count, remaining);
                inventory[i].count -= take;
                remaining -= take;
                if (inventory[i].count <= 0) inventory.RemoveAt(i);
                if (remaining <= 0) break;
            }
        }

        if (remaining > 0)
        {
            for (int i = 0; i < hotbarData.Count; i++)
            {
                if (hotbarData[i].item != null && hotbarData[i].item.itemID == data.itemID)
                {
                    int take = Mathf.Min(hotbarData[i].count, remaining);
                    hotbarData[i].count -= take;
                    remaining -= take;
                    if (hotbarData[i].count <= 0) hotbarData[i].Clear();
                    if (remaining <= 0) break;
                }
            }
        }
        RefreshAll();
    }

    public void DegradeEquippedToolDurability(float amount, int activeHotbarIndex)
    {
        if (activeHotbarIndex < 0 || activeHotbarIndex >= hotbarData.Count) return;

        HotbarSlotData slot = hotbarData[activeHotbarIndex];
        if (slot == null || slot.IsEmpty) return;

        if (slot.item != null && slot.item.maxDurability > 0)
        {
            slot.currentDurability -= amount;
            slot.currentDurability = Mathf.Max(slot.currentDurability, 0f);

            Debug.Log($"[Durability] {slot.item.itemName} at index {activeHotbarIndex} degraded to: {slot.currentDurability}/{slot.item.maxDurability}");

            if (slot.currentDurability <= 0f)
            {
                slot.count--;
                if (slot.count <= 0)
                {
                    Debug.LogWarning($"[Durability Broken] {slot.item.itemName} broke completely!");
                    slot.Clear();
                }
                else
                {
                    slot.currentDurability = slot.item.maxDurability;
                }
            }
            RefreshAll();
        }
    }

    // ==========================================
    // SAVE / LOAD SYSTEM
    // ==========================================
    public void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();

        foreach (var slot in inventory)
        {
            if (slot.item != null)
            {
                saveData.savedItems.Add(new SaveSlot
                {
                    itemId = slot.item.itemID,
                    count = slot.count,
                    currentDurability = slot.currentDurability
                });
            }
        }

        for (int i = 0; i < hotbarData.Count; i++)
        {
            var h = hotbarData[i];
            saveData.hotbarItems.Add(new HotbarSaveSlot
            {
                itemId = h.item != null ? h.item.itemID : -1,
                abilityName = h.ability != null ? h.ability.name : "",
                count = h.count,
                currentDurability = h.currentDurability
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadInventory()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

        inventory.Clear();
        foreach (var s in saveData.savedItems)
        {
            ItemData item = database.GetItemByID(s.itemId);
            if (item != null)
            {
                inventory.Add(new InventorySlot(item, s.count, s.currentDurability));
            }
        }

        InitializeHotbarData();

        for (int i = 0; i < hotbarSize; i++)
        {
            hotbarData[i].Clear();

            if (i < saveData.hotbarItems.Count)
            {
                var hSave = saveData.hotbarItems[i];

                if (hSave.itemId != -1)
                {
                    ItemData foundItem = database.GetItemByID(hSave.itemId);
                    if (foundItem != null)
                    {
                        hotbarData[i].item = foundItem;
                        hotbarData[i].count = hSave.count;
                        hotbarData[i].currentDurability = hSave.currentDurability;
                    }
                }

                if (!string.IsNullOrEmpty(hSave.abilityName) && abilityDatabase != null)
                {
                    hotbarData[i].ability = abilityDatabase.GetAbilityByName(hSave.abilityName);
                }
            }
        }
    }

    public void RefreshAll()
    {
        SaveInventory();
        InvUI.Instance?.RefreshUI();
        PlayerHotbarManager.Instance?.RefreshHotbar();
        CraftUIManager.Instance?.UpdateInfoText();
    }

    public int GetTotalCount(ItemData data)
    {
        int total = 0;
        foreach (var s in inventory)
            if (s.item != null && s.item.itemID == data.itemID) total += s.count;
        foreach (var h in hotbarData)
            if (h.item != null && h.item.itemID == data.itemID) total += h.count;
        return total;
    }

    public WorldSaveData CaptureWorldBuildingsState()
    {
        WorldSaveData worldData = new WorldSaveData();
        MonoBehaviour[] sceneObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var obj in sceneObjects)
        {
            if (obj is ISaveableBuilding building)
            {
                building.GetSaveData(out int ammo, out float progress, out int durability);

                BuildingSaveSlot buildingData = new BuildingSaveSlot
                {
                    itemId = building.GetItemID(),
                    position = obj.transform.position,
                    rotation = obj.transform.rotation,
                    ammo = ammo,
                    progress = progress,
                    currentDurability = durability
                };
                worldData.placedBuildings.Add(buildingData);
            }
        }
        return worldData;
    }
}