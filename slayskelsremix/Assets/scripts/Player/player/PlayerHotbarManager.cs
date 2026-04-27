using System;
using System.Collections;
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
    private float nextFireTime = 0f;

    private PlayerControls controls;

    public event Action<ItemData> OnSelectedItemChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        controls = new PlayerControls();
    }

    private IEnumerator Start()
    {
        selectedIndex = 0;
        yield return null;

        RefreshHotbar();
        UpdateSelector();
        NotifySelectionChanged();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Scroll.performed += ctx => HandleScroll(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // Number key selection (1–9)
        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                SelectSlot(i);
        }

        // 🔥 HOLD TO USE
        if (Keyboard.current.eKey.isPressed)
            ExecuteActiveSlot();
    }

    // =========================
    // SELECTION
    // =========================
    private void SelectSlot(int index)
    {
        if (index == selectedIndex) return;

        selectedIndex = index;
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
        if (selector != null && selectedIndex < hotbarSlots.Count)
            selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    private void NotifySelectionChanged()
    {
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

    public ItemData GetSelectedItem()
    {
        if (selectedIndex >= hotbarSlots.Count) return null;
        return hotbarSlots[selectedIndex].GetItem();
    }

    // =========================
    // HOTBAR REFRESH
    // =========================
    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null) return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < data.Count)
            {
                hotbarSlots[i].SetSlot(data[i].item, data[i].ability, data[i].count);
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
        }
    }

    // =========================
    // EXECUTION
    // =========================
    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count) return;
        if (Time.time < nextFireTime) return;

        var slot = hotbarSlots[selectedIndex];

        // =========================
        // ABILITY
        // =========================
        if (slot.GetAbility() != null)
        {
            Ability ability = slot.GetAbility();

            if (ability.Execute(caster, targetAnchor, true))
                nextFireTime = Time.time + ability.fireRate;
        }
        // =========================
        // ITEM (🔥 FIXED)
        // =========================
        else if (slot.GetItem() != null)
        {
            ItemData item = slot.GetItem();

            // Prevent using empty stacks
            if (slot.GetCount() <= 0) return;

            // Skip build items
            if (item is buildSO) return;

            // Use item (effect only — NO stack logic inside item!)
            item.Use(caster, targetAnchor);

            // 🔥 Consume ONCE
            UseSelectedStack(item.consumeAmount);

            // 🔥 Respect per-item fire rate
            nextFireTime = Time.time + item.useRate;
        }
    }

    // =========================
    // STACK USAGE
    // =========================
    public void UseSelectedStack(int amount)
    {
        if (selectedIndex >= hotbarSlots.Count) return;

        var slot = hotbarSlots[selectedIndex];

        slot.UpdateCount(-amount);

        if (slot.GetCount() <= 0)
            slot.ClearSlot();

        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }

    // =========================
    // SYNC
    // =========================
    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null) return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            while (data.Count <= i)
                data.Add(new HotbarSlotData());

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