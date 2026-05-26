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

        public InventorySlot(ItemData d = null, int c = 0, float dur = 0)
        {
            item = d;
            count = c;
            currentDurability = dur;
        }
    }

    [Header("Inventory Data")]
    public List<InventorySlot> inventory = new List<InventorySlot>();
    public List<HotbarSlotData> hotbarData = new List<HotbarSlotData>();
    public int inventorySize = 30;
    public int hotbarSize = 9;

    [Header("Dependencies")]
    public ItemDatabase database;
    public AbilityDatabase abilityDatabase;

    private string savePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/inventory.json";

        InitializeInventory();
        InitializeHotbarData();
        LoadInventory();
    }

    void InitializeInventory()
    {
        while (inventory.Count < inventorySize)
            inventory.Add(new InventorySlot());
    }

    void InitializeHotbarData()
    {
        if (hotbarData == null) hotbarData = new List<HotbarSlotData>();

        if (hotbarData.Count != hotbarSize)
        {
            hotbarData.Clear();
            for (int i = 0; i < hotbarSize; i++)
                hotbarData.Add(new HotbarSlotData());
        }
    }

    // =========================================================
    // ADD ITEM (WITH DEBUG + SAFE STACKING)
    // =========================================================

    public void AddItem(ItemData data, int amount, float customDurability = -1f)
    {
        if (data == null || amount <= 0) return;

        bool isDurabilityItem = data.maxDurability > 0;
        float durability = customDurability < 0 ? data.maxDurability : customDurability;

        Debug.Log($"[INV] ADD ITEM -> {data.itemName} x{amount} | dur:{durability}");

        // =====================================================
        // NON DURABILITY ITEMS
        // =====================================================
        if (!isDurabilityItem)
        {
            StackIntoInventory(data, amount, 0f);
            RefreshAll();
            return;
        }

        bool isFull = durability >= data.maxDurability;

        Debug.Log($"[INV] IsFullDurabilityStack: {isFull}");

        if (isFull)
        {
            StackIntoInventory(data, amount, durability);
        }
        else
        {
            InsertAsSingleItems(data, amount, durability);
        }

        RefreshAll();
    }

    // =========================================================
    // STACK LOGIC (FULL ITEMS ONLY)
    // =========================================================

    void StackIntoInventory(ItemData data, int amount, float durability)
    {
        int remaining = amount;

        Debug.Log($"[INV] STACKING into FULL durability slots...");

        // STEP 1: fill existing stacks
        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            var slot = inventory[i];

            if (slot.item != null &&
                slot.item.itemID == data.itemID &&
                slot.currentDurability >= data.maxDurability)
            {
                int space = data.maxStackSize - slot.count;
                int add = Mathf.Min(space, remaining);

                slot.count += add;
                remaining -= add;

                Debug.Log($"[INV] STACKED into slot {i} -> now {slot.count} | remaining {remaining}");
            }
        }

        // STEP 2: create new stacks
        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            if (inventory[i].item == null)
            {
                int add = Mathf.Min(remaining, data.maxStackSize);

                inventory[i].item = data;
                inventory[i].count = add;
                inventory[i].currentDurability = durability;

                remaining -= add;

                Debug.Log($"[INV] NEW STACK slot {i} -> {add} | remaining {remaining}");
            }
        }

        if (remaining > 0)
        {
            Debug.LogError($"[INV] WARNING: INVENTORY FULL -> LOST {remaining}x {data.itemName}");
        }
    }

    // =========================================================
    // DAMAGED ITEMS (ALWAYS SINGLE INSTANCES)
    // =========================================================

    void InsertAsSingleItems(ItemData data, int amount, float durability)
    {
        int remaining = amount;

        Debug.Log($"[INV] INSERTING DAMAGED ITEMS (no stacking)");

        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            if (inventory[i].item == null)
            {
                inventory[i].item = data;
                inventory[i].count = 1;
                inventory[i].currentDurability = durability;

                remaining--;

                Debug.Log($"[INV] DAMAGED ITEM -> slot {i} | dur:{durability} | remaining {remaining}");
            }
        }

        if (remaining > 0)
        {
            Debug.LogError($"[INV] WARNING: INVENTORY FULL -> LOST {remaining}x {data.itemName}");
        }
    }

    // =========================================================
    // HOTBAR RETURN SUPPORT
    // =========================================================

    public void AddSeparateItemInstance(ItemData data, int amount, float durability)
    {
        Debug.Log($"[INV] HOTBAR RETURN -> {data.itemName} x{amount}");

        InsertAsSingleItems(data, amount, durability);

        RefreshAll();
    }

    // =========================================================
    // REMOVE ITEM
    // =========================================================

    public bool RemoveItem(ItemData data, int amount)
    {
        int remaining = amount;

        Debug.Log($"[INV] REMOVE -> {data.itemName} x{amount}");

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            var slot = inventory[i];

            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                int take = Mathf.Min(slot.count, remaining);

                slot.count -= take;
                remaining -= take;

                Debug.Log($"[INV] Removed {take} from slot {i}");

                if (slot.count <= 0)
                {
                    slot.item = null;
                    slot.currentDurability = 0f;

                    Debug.Log($"[INV] Cleared slot {i}");
                }

                if (remaining <= 0)
                    break;
            }
        }

        RefreshAll();

        return remaining <= 0;
    }

    // =========================================================
    // DEBUG: SEE FULL INVENTORY STATE
    // =========================================================

    void PrintInventoryState()
    {
        Debug.Log("===== INVENTORY STATE =====");

        for (int i = 0; i < inventory.Count; i++)
        {
            var s = inventory[i];

            if (s.item == null)
            {
                Debug.Log($"Slot {i}: EMPTY");
            }
            else
            {
                Debug.Log($"Slot {i}: {s.item.itemName} x{s.count} | dur {s.currentDurability}");
            }
        }
    }

    // =========================================================
    // SAVE / LOAD (UNCHANGED LOGIC)
    // =========================================================

    public void SaveInventory()
    {
        PlayerHotbarManager.Instance?.SyncHotbarToData();

        InventorySaveData save = new InventorySaveData();

        foreach (var s in inventory)
        {
            save.savedItems.Add(new SaveSlot
            {
                itemId = s.item ? s.item.itemID : -1,
                count = s.count,
                currentDurability = s.currentDurability
            });
        }

        foreach (var h in hotbarData)
        {
            save.hotbarItems.Add(new HotbarSaveSlot
            {
                itemId = h.item ? h.item.itemID : -1,
                abilityName = h.ability ? h.ability.name : "",
                count = h.count,
                currentDurability = h.currentDurability
            });
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(save, true));

        Debug.Log("[INV] SAVED INVENTORY");
    }

    public void LoadInventory()
    {
        if (!File.Exists(savePath)) return;

        var data = JsonUtility.FromJson<InventorySaveData>(File.ReadAllText(savePath));

        InitializeInventory();

        for (int i = 0; i < inventory.Count && i < data.savedItems.Count; i++)
        {
            var s = data.savedItems[i];

            inventory[i].item = database.GetItemByID(s.itemId);
            inventory[i].count = s.count;
            inventory[i].currentDurability = s.currentDurability;
        }

        InitializeHotbarData();

        for (int i = 0; i < hotbarSize && i < data.hotbarItems.Count; i++)
        {
            var h = data.hotbarItems[i];

            hotbarData[i].item = database.GetItemByID(h.itemId);
            hotbarData[i].ability = abilityDatabase.GetAbilityByName(h.abilityName);
            hotbarData[i].count = h.count;
            hotbarData[i].currentDurability = h.currentDurability;
        }

        PlayerHotbarManager.Instance?.RefreshHotbar();

        Debug.Log("[INV] LOADED INVENTORY");
    }

    public void RefreshAll()
    {
        InvUI.Instance?.RefreshUI();
        SaveInventory();
    }

    public int GetTotalCount(ItemData data)
    {
        if (data == null) return 0;

        int total = 0;

        // inventory
        foreach (var slot in inventory)
        {
            if (slot.item != null && slot.item.itemID == data.itemID)
            {
                total += slot.count;
            }
        }

        // hotbar
        foreach (var h in hotbarData)
        {
            if (h.item != null && h.item.itemID == data.itemID)
            {
                total += h.count;
            }
        }

        Debug.Log($"[INV] GetTotalCount -> {data.itemName} = {total}");

        return total;
    }

}