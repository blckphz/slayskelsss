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
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame) SelectSlot(i);
        }
    }

    private void Start() => StartCoroutine(InitRoutine());
    private IEnumerator InitRoutine() { yield return new WaitForEndOfFrame(); RefreshHotbar(); UpdateSelector(); }

    private void OnPrimaryUse(InputAction.CallbackContext ctx) => ExecuteActiveSlot();
    private void OnSecondaryUse(InputAction.CallbackContext ctx) => ExecuteSecondaryActiveSlot();
    private void OnScroll(InputAction.CallbackContext ctx) => HandleScroll(ctx.ReadValue<Vector2>());

    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count || Time.time < nextFireTime) return;
        var slot = hotbarSlots[selectedIndex];
        if (slot.GetAbility() != null)
        {
            Ability ability = slot.GetAbility();
            if (ability.isOnCooldown) return;
            if (ability.Execute(caster, targetAnchor, true)) { nextFireTime = Time.time + ability.fireRate; StartCoroutine(AbilityCooldownRoutine(ability)); }
            return;
        }
        ItemData item = slot.GetItem();
        if (item == null || !item.isUsable || slot.GetCount() <= 0) return;
        if (item is UseableItem useableItem && useableItem.abilityToExecute != null)
        {
            Ability boundAbility = useableItem.abilityToExecute;
            if (boundAbility.isOnCooldown) return;
            if (boundAbility.Execute(caster, targetAnchor, true)) { nextFireTime = Time.time + (boundAbility is offensivemelee m ? m.swingFreq : boundAbility.fireRate); StartCoroutine(AbilityCooldownRoutine(boundAbility)); }
            return;
        }
        UseSelectedStack(item.consumeAmount);
        nextFireTime = Time.time + (item is UseableItem u ? u.useRate : 0.5f);
    }

    private IEnumerator AbilityCooldownRoutine(Ability ability)
    {
        ability.isOnCooldown = true;
        yield return new WaitForSeconds(ability.fireRate);
        ability.isOnCooldown = false;
    }

    private void ExecuteSecondaryActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count) return;
        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();
        if (item == null || !item.isUsable) return;
        if (item is UseableItem useable && useable.abilityToExecute != null)
        {
            bool isTool = useable.abilityToExecute is ToolsSO || useable.abilityToExecute is offensivemelee;
            if (!isTool && Time.time < nextFireTime) return;
            if (useable.abilityToExecute.ExecuteSecondary(caster)) { if (!isTool) nextFireTime = Time.time + useable.useRate; }
        }
    }

    public void SelectSlot(int index) { selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1); UpdateSelector(); NotifySelectionChanged(); }
    private void HandleScroll(Vector2 scroll) { selectedIndex = (scroll.y > 0) ? (selectedIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count : (selectedIndex + 1) % hotbarSlots.Count; UpdateSelector(); NotifySelectionChanged(); }
    private void UpdateSelector() { if (selector != null) selector.position = hotbarSlots[selectedIndex].transform.position; }
    private void NotifySelectionChanged() => OnSelectedItemChanged?.Invoke(GetSelectedItem());

    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null) return;
        var data = InventoryManager.Instance.hotbarData;
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < data.Count) hotbarSlots[i].SetSlot(data[i].item, data[i].ability, data[i].count, data[i].currentDurability);
        }
    }

    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null) return;
        var data = InventoryManager.Instance.hotbarData;
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i >= data.Count) data.Add(new HotbarSlotData());
            data[i].item = hotbarSlots[i].GetItem();
            data[i].ability = hotbarSlots[i].GetAbility();
            data[i].count = hotbarSlots[i].GetCount();
            data[i].currentDurability = hotbarSlots[i].GetDurability();
        }
    }

    public void ForceSyncAndSave() { SyncHotbarToData(); InventoryManager.Instance?.SaveInventory(); }
    public ItemData GetSelectedItem() => (selectedIndex >= 0 && selectedIndex < hotbarSlots.Count) ? hotbarSlots[selectedIndex].GetItem() : null;
    public void UseSelectedStack(int amount) { var slot = hotbarSlots[selectedIndex]; int newCount = slot.GetCount() - amount; if (newCount <= 0) slot.ClearSlot(); else slot.SetSlot(slot.GetItem(), slot.GetAbility(), newCount, slot.GetDurability()); ForceSyncAndSave(); }
    public float GetActiveToolDurability() => (selectedIndex >= 0 && selectedIndex < hotbarSlots.Count) ? hotbarSlots[selectedIndex].GetDurability() : 0f;
    public void ReduceActiveToolDurability(float amount) { var slot = hotbarSlots[selectedIndex]; ItemData item = slot.GetItem(); if (item == null || item.IsUnbreakable) return; float newDur = Mathf.Clamp(slot.GetDurability() - amount, 0f, item.maxDurability); if (newDur <= 0f) slot.ClearSlot(); else slot.SetSlot(item, slot.GetAbility(), slot.GetCount(), newDur); ForceSyncAndSave(); }
}