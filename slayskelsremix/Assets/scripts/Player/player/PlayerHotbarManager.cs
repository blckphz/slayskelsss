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

    [Header("References")]
    public PlayerInteraction2D interaction;

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
        controls.Player.SecondaryItemUse.performed += ctx => ExecuteSecondaryActiveSlot();
        controls.Player.Scroll.performed += ctx => HandleScroll(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // ✅ FIX: no spam holding key
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteActiveSlot();
        }

        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }
    }

    // ==========================================
    // MAIN EXECUTION
    // ==========================================
    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count || Time.time < nextFireTime)
            return;

        var slot = hotbarSlots[selectedIndex];

        // ================= ABILITY =================
        if (slot.GetAbility() != null)
        {
            Ability ability = slot.GetAbility();

            if (ability.Execute(caster, targetAnchor, true))
            {
                nextFireTime = Time.time + ability.fireRate;
            }

            return;
        }

        // ================= ITEM =================
        ItemData item = slot.GetItem();

        if (item == null || !item.isUsable || slot.GetCount() <= 0)
            return;

        float cooldown = 0.5f;

        if (item is UseableItem useable)
            cooldown = useable.useRate;

        GameObject target = interaction != null ? interaction.CurrentTarget : null;

        Debug.Log($"[Hotbar] Using item: {item.name}, Target: {(target ? target.name : "NULL")}");

        // ✅ FIX: check success BEFORE consuming
        bool success = item.Use(caster, targetAnchor, target);

        if (success)
        {
            UseSelectedStack(item.consumeAmount);
            nextFireTime = Time.time + cooldown;
        }
       
    }

    private void ExecuteSecondaryActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count || Time.time < nextFireTime)
            return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null || !item.isUsable)
            return;

        if (item is UseableItem useable && useable.abilityToExecute != null)
        {
            if (useable.abilityToExecute.ExecuteSecondary(caster))
            {
                UseSelectedStack(1);
                nextFireTime = Time.time + useable.useRate;
            }
        }
    }

    // ==========================================
    // SLOT CONTROL
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
        if (selector != null && selectedIndex < hotbarSlots.Count)
        {
            selector.position = hotbarSlots[selectedIndex].transform.position;
        }
    }

    // ==========================================
    // ITEM HELPERS
    // ==========================================
    public ItemData GetSelectedItem()
    {
        if (selectedIndex >= 0 && selectedIndex < hotbarSlots.Count)
            return hotbarSlots[selectedIndex].GetItem();

        return null;
    }

    private void NotifySelectionChanged()
    {
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

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

    // ==========================================
    // HOTBAR SYNC
    // ==========================================
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

        UpdateSelector();
    }

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