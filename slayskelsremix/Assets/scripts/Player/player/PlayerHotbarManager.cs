using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbarManager : MonoBehaviour
{
    public static PlayerHotbarManager Instance;

    public List<InventorySlotUI> hotbarSlots = new List<InventorySlotUI>();
    public RectTransform selector;
    public Transform caster;
    public Transform targetAnchor;
    public PlayerInteraction2D interaction;
    public PlayerAttack playerAttack;

    private int selectedIndex = 0;
    private PlayerControls controls;

    public event Action<ItemData> OnSelectedItemChanged;

    private float nextUseTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.PrimaryItemUse.performed += OnPrimaryUse;
        controls.Player.SecondaryItemUse.performed += OnSecondaryUse;
        controls.Player.Scroll.performed += OnScroll;
    }

    private void OnDisable()
    {
        controls.Player.PrimaryItemUse.performed -= OnPrimaryUse;
        controls.Player.SecondaryItemUse.performed -= OnSecondaryUse;
        controls.Player.Scroll.performed -= OnScroll;
        controls.Disable();
    }

    private void Update()
    {
        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                SelectSlot(i);
        }
    }

    private void OnPrimaryUse(InputAction.CallbackContext ctx)
    {
        ExecuteActiveSlot();
    }

    private void OnSecondaryUse(InputAction.CallbackContext ctx)
    {
        ExecuteSecondaryActiveSlot();
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        HandleScroll(ctx.ReadValue<Vector2>());
    }

    // =========================================================
    // PRIMARY USE
    // =========================================================
    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count) return;

        var slot = hotbarSlots[selectedIndex];
        Ability ability = slot.GetAbility();
        ItemData itemData = slot.GetItem();

        Ability abilityToExecute =
            ability != null ? ability :
            (itemData is UseableItem u ? u.abilityToExecute : null);

        // No ability → consume item only
        if (abilityToExecute == null)
        {
            if (itemData != null &&
                itemData.isUsable &&
                slot.GetCount() > 0)
            {
                UseSelectedStack(itemData.consumeAmount);
            }

            return;
        }

        float remaining =
            playerAttack != null
                ? playerAttack.GetCooldownRemaining(abilityToExecute)
                : 0f;

        if (remaining > 0f) return;
        if (Time.time < nextUseTime) return;

        bool isComboFinished =
            abilityToExecute.Execute(caster, targetAnchor, true);

        // consume item after use
        if (itemData != null &&
            itemData.isUsable &&
            itemData.consumeAmount > 0 &&
            slot.GetCount() > 0)
        {
            UseSelectedStack(itemData.consumeAmount);
        }

        float lockDuration;

        if (abilityToExecute is offensivemelee melee)
            lockDuration = isComboFinished ? melee.fireRate : melee.swingFreq;
        else
            lockDuration = abilityToExecute.fireRate > 0 ? abilityToExecute.fireRate : 0.1f;

        nextUseTime = Time.time + lockDuration;
    }

    // =========================================================
    // SECONDARY USE
    // =========================================================
    private void ExecuteSecondaryActiveSlot()
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
    // SLOT SELECTION
    // =========================================================
    public void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1);
        UpdateSelector();
        NotifySelectionChanged();
    }

    private void HandleScroll(Vector2 scroll)
    {
        selectedIndex = (scroll.y > 0)
            ? (selectedIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count
            : (selectedIndex + 1) % hotbarSlots.Count;

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
        return (selectedIndex >= 0 && selectedIndex < hotbarSlots.Count)
            ? hotbarSlots[selectedIndex].GetItem()
            : null;
    }

    // =========================================================
    // STACK USAGE
    // =========================================================
    public void UseSelectedStack(int amount)
    {
        var slot = hotbarSlots[selectedIndex];

        int newCount = slot.GetCount() - amount;

        if (newCount <= 0)
            slot.ClearSlot();
        else
            slot.SetSlot(
                slot.GetItem(),
                slot.GetAbility(),
                newCount,
                slot.GetDurability()
            );

        SyncAndSave();
    }

    // =========================================================
    // SAVE SYSTEM (CENTRALIZED)
    // =========================================================
    private void SyncAndSave()
    {
        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }

    // =========================================================
    // HOTBAR SYNC
    // =========================================================
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
    // DURABILITY (RESTORED - FIXES YOUR ERROR)
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