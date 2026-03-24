using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbarManager : MonoBehaviour
{
    public static PlayerHotbarManager Instance;

    [Header("UI References")]
    public List<InventorySlotUI> hotbarSlots = new List<InventorySlotUI>();
    public RectTransform selector;

    [Header("Combat & Interaction Context")]
    public Transform caster;
    public Transform targetAnchor;

    private int selectedIndex = 0;
    private PlayerControls controls;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
        // Standard "Use" input (e.g., Left Click or E)
        controls.Player.Use.performed += ctx => ExecuteActiveSlot();
        // Mouse Scroll for slot switching
        controls.Player.Scroll.performed += ctx => HandleScroll(ctx.ReadValue<Vector2>());
    }

    private void OnDisable() => controls.Disable();

    private void Update()
    {
        // Numerical Hotkeys (1-9)
        for (int i = 0; i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame) SelectSlot(i);
        }
    }

    // ---------------- SLOT SELECTION ----------------

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbarSlots.Count) return;
        selectedIndex = index;
        UpdateSelector();
    }

    private void HandleScroll(Vector2 scrollVector)
    {
        if (scrollVector.y > 0) selectedIndex = (selectedIndex - 1 + 9) % 9;
        else if (scrollVector.y < 0) selectedIndex = (selectedIndex + 1) % 9;
        UpdateSelector();
    }

    public void UpdateSelector()
    {
        if (selector == null || hotbarSlots.Count <= selectedIndex) return;
        selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    // ---------------- DATA ACCESS (For BuildManager) ----------------

    /// <summary>
    /// Returns the ItemData currently highlighted by the selector.
    /// </summary>
    public ItemData GetSelectedItem()
    {
        if (hotbarSlots.Count <= selectedIndex) return null;
        return hotbarSlots[selectedIndex].GetItem();
    }

    /// <summary>
    /// Returns the stack count of the currently selected slot.
    /// </summary>
    public int GetSelectedCount()
    {
        if (hotbarSlots.Count <= selectedIndex) return 0;
        return hotbarSlots[selectedIndex].GetCount();
    }

    // ---------------- EXECUTION & CONSUMPTION ----------------

    public void ExecuteActiveSlot()
    {
        if (hotbarSlots.Count <= selectedIndex) return;
        InventorySlotUI slot = hotbarSlots[selectedIndex];

        // 1. Check for Ability (Class skills)
        if (slot.GetAbility() != null)
        {
            slot.GetAbility().Execute(caster, targetAnchor, true);
        }
        // 2. Check for Item (Consumables/Tools)
        // Note: BuildItems are handled by BuildManager, but we check here for generic 'Use'
        else if (slot.GetItem() != null)
        {
            // If it's a build item, we don't 'Use' it like a potion, 
            // the BuildManager handles the Left Click logic.
            if (!(slot.GetItem() is buildSO))
            {
                slot.GetItem().Use(caster, targetAnchor);
            }
        }
    }

    /// <summary>
    /// Reduces the item count in the active slot (used by BuildManager after placement).
    /// </summary>
    public void UseSelectedStack(int amount)
    {
        if (hotbarSlots.Count <= selectedIndex) return;

        InventorySlotUI slot = hotbarSlots[selectedIndex];

        // Update the UI visual count
        slot.UpdateCount(-amount);

        // If count hits zero, clear the visual slot
        if (slot.GetCount() <= 0)
        {
            slot.ClearSlot();
        }

        // Save the change back to the main inventory data
        SyncHotbarToData();
    }

    // ---------------- DATA SYNCING ----------------

    public bool IsAlreadyInHotbar(ItemData item, Ability ability, InventorySlotUI excludingSlot)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot == excludingSlot) continue;
            if (item != null && slot.GetItem() == item) return true;
            if (ability != null && slot.GetAbility() == ability) return true;
        }
        return false;
    }

    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null) return;
        var dataList = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            while (dataList.Count <= i) dataList.Add(new HotbarSlotData());

            dataList[i].item = hotbarSlots[i].GetItem();
            dataList[i].ability = hotbarSlots[i].GetAbility();
            dataList[i].count = hotbarSlots[i].GetCount();
        }
        InventoryManager.Instance.SaveInventory();
    }

    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null) return;
        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < data.Count)
                hotbarSlots[i].SetSlot(data[i].item, data[i].ability, data[i].count);
            else
                hotbarSlots[i].ClearSlot();
        }
    }
}