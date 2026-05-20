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

    // Prevents input spam
    private float nextUseTime = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

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

    // ====================================
    // PRIMARY ITEM / ABILITY EXECUTION
    // ====================================
    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count)
        {
            Debug.Log("[HOTBAR] Invalid slot");
            return;
        }

        var slot = hotbarSlots[selectedIndex];
        Ability ability = slot.GetAbility();
        ItemData itemData = slot.GetItem();

        // Determine ability to execute
        Ability abilityToExecute =
            ability != null
            ? ability
            : (itemData is UseableItem u ? u.abilityToExecute : null);

        // --------------------------------------------------
        // CASE 1: No ability -> just consume usable item
        // --------------------------------------------------
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

        // --------------------------------------------------
        // Check cooldown
        // --------------------------------------------------
        float remaining =
            playerAttack != null
            ? playerAttack.GetCooldownRemaining(abilityToExecute)
            : 0f;

        if (remaining > 0f)
        {
            Debug.Log($"[HOTBAR] Blocked: Cooldown {remaining:F2}s remaining");
            return;
        }

        // --------------------------------------------------
        // Check anti-spam lock
        // --------------------------------------------------
        if (Time.time < nextUseTime)
        {
            Debug.Log("[HOTBAR] Blocked: Input spam lock");
            return;
        }

        // --------------------------------------------------
        // Execute ability
        // --------------------------------------------------
        bool isComboFinished =
            abilityToExecute.Execute(caster, targetAnchor, true);

        // --------------------------------------------------
        // Consume item stack AFTER successful execution
        // Only if consumeAmount > 0
        // --------------------------------------------------
        if (itemData != null &&
            itemData.isUsable &&
            itemData.consumeAmount > 0 &&
            slot.GetCount() > 0)
        {
            UseSelectedStack(itemData.consumeAmount);
        }

        // --------------------------------------------------
        // Determine lock duration
        // --------------------------------------------------
        float lockDuration;

        if (abilityToExecute is offensivemelee melee)
        {
            // Combo system:
            // swingFreq between swings
            // fireRate after final swing
            lockDuration = isComboFinished
                ? melee.fireRate
                : melee.swingFreq;
        }
        else
        {
            lockDuration = abilityToExecute.fireRate > 0
                ? abilityToExecute.fireRate
                : 0.1f;
        }

        nextUseTime = Time.time + lockDuration;

        Debug.Log(
            $"[HOTBAR] Executed {abilityToExecute.abilityName}. " +
            $"Lock: {lockDuration:F2}s"
        );
    }

    // ====================================
    // SECONDARY USE
    // ====================================
    private void ExecuteSecondaryActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count)
            return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null || !item.isUsable)
            return;

        if (item is UseableItem useable &&
            useable.abilityToExecute != null)
        {
            Debug.Log(
                $"[HOTBAR SECONDARY] " +
                $"{useable.abilityToExecute.abilityName}"
            );

            useable.abilityToExecute.ExecuteSecondary(caster);
        }
    }

    // ====================================
    // SLOT SELECTION
    // ====================================
    public void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(
            index,
            0,
            hotbarSlots.Count - 1
        );

        Debug.Log($"[HOTBAR] Selected {selectedIndex}");

        UpdateSelector();
        NotifySelectionChanged();
    }

    private void HandleScroll(Vector2 scroll)
    {
        selectedIndex = (scroll.y > 0)
            ? (selectedIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count
            : (selectedIndex + 1) % hotbarSlots.Count;

        Debug.Log($"[HOTBAR] Scroll -> {selectedIndex}");

        UpdateSelector();
        NotifySelectionChanged();
    }

    private void UpdateSelector()
    {
        if (selector != null &&
            selectedIndex < hotbarSlots.Count)
        {
            selector.position =
                hotbarSlots[selectedIndex].transform.position;
        }
    }

    private void NotifySelectionChanged()
    {
        OnSelectedItemChanged?.Invoke(
            GetSelectedItem()
        );
    }

    public ItemData GetSelectedItem()
    {
        return (selectedIndex >= 0 &&
                selectedIndex < hotbarSlots.Count)
            ? hotbarSlots[selectedIndex].GetItem()
            : null;
    }

    // ====================================
    // STACK MANAGEMENT
    // ====================================
    public void UseSelectedStack(int amount)
    {
        var slot = hotbarSlots[selectedIndex];

        int newCount = slot.GetCount() - amount;

        if (newCount <= 0)
        {
            slot.ClearSlot();
        }
        else
        {
            slot.SetSlot(
                slot.GetItem(),
                slot.GetAbility(),
                newCount,
                slot.GetDurability()
            );
        }
    }

    // ====================================
    // INVENTORY SYNC
    // ====================================
    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null)
            return;

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

        Debug.Log("[HOTBAR] Refreshed");
    }

    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null)
            return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i >= data.Count)
                data.Add(new HotbarSlotData());

            data[i].item = hotbarSlots[i].GetItem();
            data[i].ability = hotbarSlots[i].GetAbility();
            data[i].count = hotbarSlots[i].GetCount();
            data[i].currentDurability =
                hotbarSlots[i].GetDurability();
        }

        Debug.Log("[HOTBAR] Synced");
    }

    // ====================================
    // DEBUG
    // ====================================
    public void PrintState()
    {
        Debug.Log("===== HOTBAR STATE =====");

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            var a = hotbarSlots[i].GetAbility();
            var it = hotbarSlots[i].GetItem();

            Debug.Log(
                $"Slot {i}: " +
                $"{(a ? a.abilityName : "None")} | " +
                $"{(it ? it.name : "None")}"
            );
        }
    }

    public float GetActiveToolDurability()
    {
        if (selectedIndex < 0 ||
            selectedIndex >= hotbarSlots.Count)
            return 0f;

        return hotbarSlots[selectedIndex]
            .GetDurability();
    }

    public void ReduceActiveToolDurability(float amount)
    {
        if (selectedIndex < 0 ||
            selectedIndex >= hotbarSlots.Count)
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

        Debug.Log(
            $"[DURABILITY] " +
            $"Before={slot.GetDurability()} " +
            $"After={newDur}"
        );

        if (newDur <= 0f)
        {
            Debug.Log(
                "[DURABILITY] Item broke -> clearing slot"
            );

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

        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }
}