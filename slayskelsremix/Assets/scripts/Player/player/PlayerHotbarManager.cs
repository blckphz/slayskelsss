using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbarManager : MonoBehaviour
{
    public static PlayerHotbarManager Instance;

    [Header("Hotbar")]
    public List<InventorySlotUI> hotbarSlots = new List<InventorySlotUI>();
    public RectTransform selector;

    [Header("References")]
    public Transform caster;
    public Transform targetAnchor;
    public PlayerAttack playerAttack;

    private int selectedIndex = 0;
    private PlayerControls controls;

    public event Action<ItemData> OnSelectedItemChanged;

    private float nextUseTime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Player.PrimaryItemUse.performed += OnPrimaryUse;
        controls.Player.SecondaryItemUse.performed += OnSecondaryUse;
        controls.Player.Scroll.performed += OnScroll;
    }

    void OnDisable()
    {
        controls.Player.PrimaryItemUse.performed -= OnPrimaryUse;
        controls.Player.SecondaryItemUse.performed -= OnSecondaryUse;
        controls.Player.Scroll.performed -= OnScroll;
        controls.Disable();
    }

    void Update()
    {
        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                SelectSlot(i);
        }
    }

    void OnPrimaryUse(InputAction.CallbackContext ctx) => ExecuteActiveSlot();
    void OnSecondaryUse(InputAction.CallbackContext ctx) => ExecuteSecondaryActiveSlot();
    void OnScroll(InputAction.CallbackContext ctx) => HandleScroll(ctx.ReadValue<Vector2>());

    // =========================================================
    // PRIMARY USE
    // =========================================================
    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count) return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null) return;

        // ❌ GLOBAL BLOCK: build mode disables combat/ability use
        if (BuildState.IsBuildMode)
        {
            // allow tools like shovel ONLY if needed elsewhere
            if (item is ShowelSO)
                return;

            return;
        }

        Ability ability =
            slot.GetAbility() ??
            (item is UseableItem u ? u.abilityToExecute : null);

        if (ability == null)
        {
            if (item.itemType == ItemType.Constructable)
                return;

            if (item.isUsable && slot.GetCount() > 0)
                UseSelectedStack(item.consumeAmount);

            return;
        }

        float remaining =
            playerAttack != null
                ? playerAttack.GetCooldownRemaining(ability)
                : 0f;

        if (remaining > 0f) return;
        if (Time.time < nextUseTime) return;

        ability.Execute(caster, targetAnchor, true);

        if (item.itemType != ItemType.Constructable && item.isUsable && slot.GetCount() > 0)
            UseSelectedStack(item.consumeAmount);

        nextUseTime = Time.time + Mathf.Max(ability.fireRate, 0.1f);
    }

    // =========================================================
    // SECONDARY USE
    // =========================================================
    void ExecuteSecondaryActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count) return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item is UseableItem useable &&
            useable.abilityToExecute != null)
        {
            useable.abilityToExecute.ExecuteSecondary(caster);
        }
    }

    // =========================================================
    // SLOT CONTROL
    // =========================================================
    public void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1);
        UpdateSelector();
        NotifySelectionChanged();
    }

    void HandleScroll(Vector2 scroll)
    {
        selectedIndex = (scroll.y > 0)
            ? (selectedIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count
            : (selectedIndex + 1) % hotbarSlots.Count;

        UpdateSelector();
        NotifySelectionChanged();
    }

    void UpdateSelector()
    {
        if (selector != null && selectedIndex < hotbarSlots.Count)
            selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    void NotifySelectionChanged()
    {
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

    public ItemData GetSelectedItem()
    {
        return (selectedIndex >= 0 && selectedIndex < hotbarSlots.Count)
            ? hotbarSlots[selectedIndex].GetItem()
            : null;
    }

    // =========================================================
    // STACK SYSTEM
    // =========================================================
    public void UseSelectedStack(int amount)
    {
        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();
        if (item == null) return;

        int newCount = slot.GetCount() - amount;

        if (newCount <= 0)
            slot.ClearSlot();
        else
            slot.SetSlot(item, slot.GetAbility(), newCount, slot.GetDurability());

        SyncAndSave();
    }

    void SyncAndSave()
    {
        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }

    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null) return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i >= data.Count)
                data.Add(new HotbarSlotData());

            data[i].item = hotbarSlots[i].GetItem();
            data[i].ability = hotbarSlots[i].GetAbility();
            data[i].count = hotbarSlots[i].GetCount();
            data[i].currentDurability = hotbarSlots[i].GetDurability();
        }
    }
    // =========================================================
    // UI / EXTERNAL SYNC (RESTORED)
    // =========================================================
    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null) return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < data.Count)
            {
                hotbarSlots[i].SetSlot(
                    data[i].item,
                    data[i].ability,
                    data[i].count,
                    data[i].currentDurability
                );
            }
        }
    }

    // =========================================================
    // DURABILITY SYSTEM (RESTORED)
    // =========================================================
    public float GetActiveToolDurability()
    {
        if (selectedIndex < 0 || selectedIndex >= hotbarSlots.Count)
            return 0f;

        return hotbarSlots[selectedIndex].GetDurability();
    }

    public void ReduceActiveToolDurability(float amount)
    {
        if (selectedIndex < 0 || selectedIndex >= hotbarSlots.Count)
            return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null || item.IsUnbreakable)
            return;

        float newDur = Mathf.Clamp(
            slot.GetDurability() - amount,
            0f,
            item.maxDurability
        );

        if (newDur <= 0f)
        {
            slot.ClearSlot();
        }
        else
        {
            slot.SetSlot(
                item,
                slot.GetAbility(),
                slot.GetCount(),
                newDur
            );
        }

        SyncAndSave();
    }

}