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

    private int selectedIndex = 0;
    private PlayerControls controls;

    public event Action<ItemData> OnSelectedItemChanged;

    private bool primaryQueued;
    private bool secondaryQueued;

    // simple anti-spam for build mode message
    private float lastBuildModeMessageTime;
    private const float buildModeMessageCooldown = 1f;

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

    void OnPrimaryUse(InputAction.CallbackContext ctx) => primaryQueued = true;
    void OnSecondaryUse(InputAction.CallbackContext ctx) => secondaryQueued = true;

    void OnScroll(InputAction.CallbackContext ctx)
    {
        HandleScroll(ctx.ReadValue<Vector2>());
    }

    void Update()
    {
        if (primaryQueued && !ActionLock.IsLocked)
        {
            primaryQueued = false;
            ExecuteActiveSlot();
        }

        if (secondaryQueued && !ActionLock.IsLocked)
        {
            secondaryQueued = false;
            ExecuteSecondaryActiveSlot();
        }

        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                SelectSlot(i);
        }
    }

    // ================= CORE FIX =================

    void EnsureSingleItemInHotbar()
    {
        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null) return;

        int count = slot.GetCount();

        if (count <= 1) return;

        slot.SetSlot(item, slot.GetAbility(), 1, slot.GetDurability());

        InventoryManager.Instance.AddItem(item, count - 1, slot.GetDurability());
    }

    // ================= EXECUTION =================

    public void ExecuteActiveSlot()
    {
        EnsureSingleItemInHotbar();

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null)
            return;

        // 🔥 BUILD MODE BLOCK WITH FEEDBACK
        if (BuildState.IsBuildMode)
        {
            if (Time.time - lastBuildModeMessageTime > buildModeMessageCooldown)
            {
                InteractionUI.Instance?.Show("Currently building");
                lastBuildModeMessageTime = Time.time;
            }
            return;
        }

        Ability ability =
            slot.GetAbility() ??
            (item is UseableItem u ? u.abilityToExecute : null);

        if (ability == null)
            return;

        bool success = ability.Execute(caster, targetAnchor, true);

        if (success)
            ReduceActiveToolDurability(1f);
    }

    void ExecuteSecondaryActiveSlot()
    {
        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item is UseableItem useable && useable.abilityToExecute != null)
        {
            useable.abilityToExecute.ExecuteSecondary(caster);
        }
    }

    // ================= DURABILITY =================

    public void ReduceActiveToolDurability(float amount)
    {
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
            slot.SetSlot(item, slot.GetAbility(), 1, newDur);
        }

        SyncHotbarToData();
    }

    // ================= COMPATIBILITY =================

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

        SyncHotbarToData();
    }

    public float GetActiveToolDurability()
    {
        var slot = hotbarSlots[selectedIndex];
        return slot != null ? slot.GetDurability() : 0f;
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

    // ================= SELECTION =================

    public void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1);
        UpdateSelector();
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

    void HandleScroll(Vector2 scroll)
    {
        selectedIndex = (scroll.y > 0)
            ? (selectedIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count
            : (selectedIndex + 1) % hotbarSlots.Count;

        UpdateSelector();
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

    void UpdateSelector()
    {
        if (selector != null)
            selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    public ItemData GetSelectedItem()
    {
        return hotbarSlots[selectedIndex].GetItem();
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
}