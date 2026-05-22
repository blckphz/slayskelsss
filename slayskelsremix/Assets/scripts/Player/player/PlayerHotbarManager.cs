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

    // =========================================================
    // INPUT
    // =========================================================
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

        if (item == null)
        {
            Debug.Log("[Hotbar] No item selected.");
            return;
        }

        Debug.Log($"[Hotbar] Using: {item.itemName} | Type: {item.itemType}");

        Ability ability =
            slot.GetAbility() ??
            (item is UseableItem u ? u.abilityToExecute : null);

        // =====================================================
        // NO ABILITY CASE
        // =====================================================
        if (ability == null)
        {
            // 🔥 IMPORTANT FIX: do NOT consume Placeable items here
            if (item.itemType == ItemType.Placeable)
            {
                Debug.Log("[Hotbar] Placeable item used → skipping consumption (handled by BuildManager)");
                return;
            }

            if (item.isUsable && slot.GetCount() > 0)
            {
                Debug.Log($"[Hotbar] Consuming item (no ability): {item.itemName}");
                UseSelectedStack(item.consumeAmount);
            }

            return;
        }

        // =====================================================
        // ABILITY COOLDOWN CHECK
        // =====================================================
        float remaining =
            playerAttack != null
                ? playerAttack.GetCooldownRemaining(ability)
                : 0f;

        if (remaining > 0f)
        {
            Debug.Log("[Hotbar] Ability on cooldown.");
            return;
        }

        if (Time.time < nextUseTime)
            return;

        // =====================================================
        // EXECUTE ABILITY
        // =====================================================
        bool finished = ability.Execute(caster, targetAnchor, true);

        Debug.Log($"[Hotbar] Ability executed: {ability.name} | finished: {finished}");

        // =====================================================
        // CONSUME AFTER ABILITY (ONLY NON-PLACEABLE LOGIC)
        // =====================================================
        if (item.itemType != ItemType.Placeable && item.isUsable && slot.GetCount() > 0)
        {
            Debug.Log($"[Hotbar] Consuming after ability: {item.itemName}");
            UseSelectedStack(item.consumeAmount);
        }

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
            Debug.Log($"[Hotbar] Secondary use: {item.itemName}");
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

        Debug.Log($"[Hotbar] Selected slot: {selectedIndex}");
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

        Debug.Log($"[Hotbar] Consume: {item.itemName} | -{amount} → {newCount}");

        if (newCount <= 0)
        {
            Debug.Log($"[Hotbar] Slot cleared: {item.itemName}");
            slot.ClearSlot();
        }
        else
        {
            slot.SetSlot(
                item,
                slot.GetAbility(),
                newCount,
                slot.GetDurability()
            );
        }

        SyncAndSave();
    }

    // =========================================================
    // SAVE / SYNC
    // =========================================================
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

    // =========================================================
    // DURABILITY
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
            Debug.Log($"[Hotbar] Broken: {item.itemName}");
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