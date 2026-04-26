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
        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                SelectSlot(i);
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
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
    // HOTBAR REFRESH (IMPORTANT FIX)
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

        if (slot.GetAbility() != null)
        {
            Ability ability = slot.GetAbility();

            if (ability.Execute(caster, targetAnchor, true))
                nextFireTime = Time.time + ability.fireRate;
        }
        else if (slot.GetItem() != null)
        {
            ItemData item = slot.GetItem();

            if (!(item is buildSO))
            {
                item.Use(caster, targetAnchor);
                nextFireTime = Time.time + 0.2f;
            }
        }
    }

    // =========================
    // STACK USAGE (🔥 FIXED CORE ISSUE)
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
    // 🔥 SINGLE SOURCE OF TRUTH FIX
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