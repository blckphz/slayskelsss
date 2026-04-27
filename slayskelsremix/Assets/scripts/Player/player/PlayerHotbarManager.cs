using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbarManager : MonoBehaviour
{
    public static PlayerHotbarManager Instance;

    [Header("UI & Context")]
    public List<InventorySlotUI> hotbarSlots = new List<InventorySlotUI>();
    public RectTransform selector;
    public Transform caster;
    public Transform targetAnchor;

    private int selectedIndex = 0;
    private float nextFireTime = 0f;
    private PlayerControls controls;

    // Event for other systems (like BuildManager) to know when the selection changes
    public event Action<ItemData> OnSelectedItemChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        controls = new PlayerControls();
    }

    private IEnumerator Start()
    {
        // Set default selection
        selectedIndex = 0;

        // Wait one frame to ensure InventoryManager has finished LoadInventory()
        yield return null;

        RefreshHotbar();
        UpdateSelector();
        NotifySelectionChanged();
    }

    private void OnEnable()
    {
        controls.Enable();
        // Bind the Secondary Use (Eating/Consuming)
        controls.Player.SecondaryItemUse.performed += ctx => ExecuteSecondaryActiveSlot();
        // Bind Scroll Wheel
        controls.Player.Scroll.performed += ctx => HandleScroll(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // 🔥 PRIMARY USE (Hold 'E' to use/throw)
        if (Keyboard.current.eKey.isPressed)
        {
            ExecuteActiveSlot();
        }

        // Hotbar selection via Number Keys (1-9)
        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }
    }

    // ==========================================
    // EXECUTION LOGIC (PRIMARY & SECONDARY)
    // ==========================================

    public void ExecuteActiveSlot()
    {
        // Rate limiting and safety checks
        if (selectedIndex >= hotbarSlots.Count || Time.time < nextFireTime) return;

        var slot = hotbarSlots[selectedIndex];

        // 1. Check for Ability (Direct Ability slots)
        if (slot.GetAbility() != null)
        {
            Ability ability = slot.GetAbility();
            if (ability.Execute(caster, targetAnchor, true))
            {
                nextFireTime = Time.time + ability.fireRate;
            }
        }
        // 2. Check for Item (Standard Inventory Items)
        else if (slot.GetItem() != null)
        {
            ItemData item = slot.GetItem();

            // Safety: Don't use if stack is empty or if it's a building ghost
            if (slot.GetCount() <= 0) return;
            if (item is buildSO) return;

            // Execute the item effect (Throwing, etc.)
            item.Use(caster, targetAnchor);

            // Consume the item from the stack
            UseSelectedStack(item.consumeAmount);

            // Set cooldown based on item use rate
            nextFireTime = Time.time + item.useRate;
        }
    }

    private void ExecuteSecondaryActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count || Time.time < nextFireTime) return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        // Bridges ItemData to Ability secondary (like the Berry Healing)
        if (item is UseableItem useable && useable.abilityToExecute != null)
        {
            // ExecuteSecondary returns true if the action (healing) was actually successful
            if (useable.abilityToExecute.ExecuteSecondary(caster))
            {
                UseSelectedStack(1);
                nextFireTime = Time.time + useable.useRate;
            }
        }
        else
        {
            Debug.Log("<color=orange>[Hotbar]</color> No secondary use for this item.");
        }
    }

    // ==========================================
    // SELECTION & VISUALS
    // ==========================================

    private void SelectSlot(int index)
    {
        if (index == selectedIndex) return;

        selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1);
        UpdateSelector();
        NotifySelectionChanged();
    }

    private void HandleScroll(Vector2 scroll)
    {
        if (hotbarSlots.Count == 0) return;

        if (scroll.y > 0)
            selectedIndex = (selectedIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count;
        else if (scroll.y < 0)
            selectedIndex = (selectedIndex + 1) % hotbarSlots.Count;

        UpdateSelector();
        NotifySelectionChanged();
    }

    private void UpdateSelector()
    {
        // Restored original positioning logic
        if (selector != null && selectedIndex >= 0 && selectedIndex < hotbarSlots.Count)
        {
            selector.position = hotbarSlots[selectedIndex].transform.position;
        }
    }

    public ItemData GetSelectedItem()
    {
        if (selectedIndex >= 0 && selectedIndex < hotbarSlots.Count)
        {
            return hotbarSlots[selectedIndex].GetItem();
        }
        return null;
    }

    private void NotifySelectionChanged()
    {
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

    // ==========================================
    // DATA SYNC & PERSISTENCE
    // ==========================================

    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null) return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < data.Count)
            {
                // Push data from InventoryManager to the UI Slots
                hotbarSlots[i].SetSlot(data[i].item, data[i].ability, data[i].count);
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
        }
        UpdateSelector();
    }

    public void UseSelectedStack(int amount)
    {
        if (selectedIndex >= hotbarSlots.Count) return;

        var slot = hotbarSlots[selectedIndex];
        slot.UpdateCount(-amount);

        if (slot.GetCount() <= 0)
        {
            slot.ClearSlot();
        }

        // IMPORTANT: Tell the data layer to update and save the file
        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }

    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null) return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            // Ensure the data list matches the slot count
            while (data.Count <= i) data.Add(new HotbarSlotData());

            data[i].item = hotbarSlots[i].GetItem();
            data[i].ability = hotbarSlots[i].GetAbility();
            data[i].count = hotbarSlots[i].GetCount();
        }
    }

    public void ForceSyncAndSave()
    {
        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }
}