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

    private bool primaryQueued;
    private bool secondaryQueued;

    void Awake()
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

    void OnPrimaryUse(InputAction.CallbackContext ctx)
    {
        Debug.Log("[HOTBAR] Primary queued");
        primaryQueued = true;
    }

    void OnSecondaryUse(InputAction.CallbackContext ctx)
    {
        Debug.Log("[HOTBAR] Secondary queued");
        secondaryQueued = true;
    }

    void OnScroll(InputAction.CallbackContext ctx)
    {
        HandleScroll(ctx.ReadValue<Vector2>());
    }

    void Update()
    {
        if (ActionLock.IsLocked)
        {
            primaryQueued = false;
            secondaryQueued = false;
        }

        if (primaryQueued)
        {
            primaryQueued = false;
            Debug.Log("[HOTBAR] Execute PRIMARY");
            ExecuteActiveSlot();
        }

        if (secondaryQueued)
        {
            secondaryQueued = false;
            Debug.Log("[HOTBAR] Execute SECONDARY");
            ExecuteSecondaryActiveSlot();
        }

        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                Debug.Log($"[HOTBAR] Number key {i}");
                SelectSlot(i);
            }
        }
    }

    public void ExecuteActiveSlot()
    {
        if (ActionLock.IsLocked)
        {
            Debug.Log("[HOTBAR] BLOCKED primary");
            return;
        }

        if (selectedIndex >= hotbarSlots.Count)
        {
            Debug.Log("[HOTBAR] Invalid index");
            return;
        }

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        Debug.Log($"[HOTBAR] Use slot {selectedIndex} item: {(item ? item.itemName : "NULL")} count: {slot.GetCount()}");

        if (item == null)
        {
            Debug.Log("[HOTBAR] No item");
            return;
        }

        if (BuildState.IsBuildMode)
        {
            Debug.Log("[HOTBAR] Blocked build mode");
            return;
        }

        Ability ability =
            slot.GetAbility() ??
            (item is UseableItem u ? u.abilityToExecute : null);

        Debug.Log($"[HOTBAR] Ability: {(ability ? ability.name : "NULL")}");

        if (ability == null)
        {
            Debug.Log("[HOTBAR] No ability");
            return;
        }

        ability.Execute(caster, targetAnchor, true);

        Debug.Log("[HOTBAR] Ability executed (NO CONSUMPTION)");

        nextUseTime = Time.time + Mathf.Max(ability.fireRate, 0.1f);
    }

    void ExecuteSecondaryActiveSlot()
    {
        if (ActionLock.IsLocked)
        {
            Debug.Log("[HOTBAR] BLOCKED secondary");
            return;
        }

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        Debug.Log($"[HOTBAR] Secondary item: {(item ? item.itemName : "NULL")}");

        if (item is UseableItem useable && useable.abilityToExecute != null)
        {
            useable.abilityToExecute.ExecuteSecondary(caster);
            Debug.Log("[HOTBAR] Secondary executed");
        }
    }

    public void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1);
        UpdateSelector();
        NotifySelectionChanged();

        Debug.Log($"[HOTBAR] Selected {selectedIndex}");
    }

    void HandleScroll(Vector2 scroll)
    {
        selectedIndex = (scroll.y > 0)
            ? (selectedIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count
            : (selectedIndex + 1) % hotbarSlots.Count;

        UpdateSelector();
        NotifySelectionChanged();

        Debug.Log($"[HOTBAR] Scroll -> {selectedIndex}");
    }

    void UpdateSelector()
    {
        if (selector != null)
            selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    void NotifySelectionChanged()
    {
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

    public ItemData GetSelectedItem()
    {
        return hotbarSlots[selectedIndex].GetItem();
    }

    // ================= STACK SYSTEM =================

    public void UseSelectedStack(int amount)
    {
        Debug.Log($"[HOTBAR] UseSelectedStack {amount}");

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null)
        {
            Debug.Log("[HOTBAR] Stack NULL item");
            return;
        }

        int newCount = slot.GetCount() - amount;

        Debug.Log($"[HOTBAR] Stack {slot.GetCount()} -> {newCount}");

        if (newCount <= 0)
        {
            Debug.Log("[HOTBAR] Clearing slot");
            slot.ClearSlot();
        }
        else
        {
            slot.SetSlot(item, slot.GetAbility(), newCount, slot.GetDurability());
        }

        SyncAndSave();
    }

    // ================= SYNC =================

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

    // ================= DURABILITY (RESTORED) =================

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

        Debug.Log($"[HOTBAR] Durability {slot.GetDurability()} -> {newDur}");

        if (newDur <= 0f)
            slot.ClearSlot();
        else
            slot.SetSlot(item, slot.GetAbility(), slot.GetCount(), newDur);

        SyncAndSave();
    }
}