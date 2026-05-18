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
        else
        {
            Destroy(gameObject);
            return;
        }

        controls = new PlayerControls();

        foreach (var slot in hotbarSlots)
        {
            if (slot != null && slot.GetAbility() != null)
            {
                slot.GetAbility().isOnCooldown = false;
            }
        }

    }

    private IEnumerator Start()
    {
        selectedIndex = 0;
        yield return null;

        Debug.Log("[Hotbar] Start -> Refreshing hotbar");

        RefreshHotbar();
        UpdateSelector();
        NotifySelectionChanged();
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
            {
                SelectSlot(i);
            }
        }
    }

    private void OnPrimaryUse(InputAction.CallbackContext ctx) => ExecuteActiveSlot();
    private void OnSecondaryUse(InputAction.CallbackContext ctx) => ExecuteSecondaryActiveSlot();
    private void OnScroll(InputAction.CallbackContext ctx) => HandleScroll(ctx.ReadValue<Vector2>());

    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count) return;
        if (Time.time < nextFireTime) return;

        var slot = hotbarSlots[selectedIndex];

        // CHECK 1: DIRECT ABILITY SLOTS
        if (slot.GetAbility() != null)
        {
            Ability ability = slot.GetAbility();
            if (ability.isOnCooldown) return;

            if (ability.Execute(caster, targetAnchor, true))
            {
                nextFireTime = Time.time + ability.fireRate;
                StartCoroutine(AbilityCooldownRoutine(ability));
            }
            return;
        }

        // CHECK 2: ITEM SLOTS
        ItemData item = slot.GetItem();
        if (item == null) return;

        if (!item.isUsable || slot.GetCount() <= 0) return;

        if (item is UseableItem useableItem && useableItem.abilityToExecute != null)
        {
            Ability boundAbility = useableItem.abilityToExecute;
            if (boundAbility.isOnCooldown) return;

            if (boundAbility.Execute(caster, targetAnchor, true))
            {
                if (boundAbility is offensivemelee melee)
                {
                    nextFireTime = Time.time + melee.swingFreq;
                }
                else
                {
                    nextFireTime = Time.time + boundAbility.fireRate;
                    StartCoroutine(AbilityCooldownRoutine(boundAbility));
                }
            }
            return;
        }

        // CHECK 3: CONSUMABLES
        float defaultCooldown = item is UseableItem u ? u.useRate : 0.5f;
        UseSelectedStack(item.consumeAmount);
        nextFireTime = Time.time + defaultCooldown;
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

            if (useable.abilityToExecute.ExecuteSecondary(caster))
            {
                if (!isTool) nextFireTime = Time.time + useable.useRate;
            }
        }
    }

    // ================= DATA LAYER GETTERS =================

    /// <summary>
    /// Gets the runtime durability value of the currently active tool slot.
    /// Used by health tracking logs.
    /// </summary>
    public float GetActiveToolDurability()
    {
        if (selectedIndex >= hotbarSlots.Count) return 0f;
        return hotbarSlots[selectedIndex].GetDurability();
    }

    // ================= DURABILITY DAMAGE HANDLER (ON-KILL ONLY) =================

    /// <summary>
    /// Decays active weapon durability. Call this exclusively from enemy death events!
    /// </summary>
    public void ReduceActiveToolDurability(float amount)
    {
        if (selectedIndex >= hotbarSlots.Count) return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        // FIX: Safely exit early if there is no item or if it's flagged as unbreakable
        if (item == null || item.IsUnbreakable) return;

        float updatedDurability = slot.GetDurability() - amount;
        updatedDurability = Mathf.Clamp(updatedDurability, 0f, item.maxDurability);

        if (updatedDurability <= 0f)
        {
            Debug.Log($"<color=red>[Item Broken]</color> {item.itemName} shattered from a fatal blow!");

            int currentStack = slot.GetCount() - 1;
            if (currentStack <= 0)
            {
                slot.ClearSlot();
            }
            else
            {
                slot.SetSlot(item, slot.GetAbility(), currentStack, item.maxDurability);
            }
        }
        else
        {
            slot.SetSlot(item, slot.GetAbility(), slot.GetCount(), updatedDurability);
        }

        SyncHotbarToData();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SaveInventory();
        }

        NotifySelectionChanged();
    }

    // ================= SLOT MANAGEMENT & UTILS =================

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

    public void UseSelectedStack(int amount)
    {
        if (selectedIndex >= hotbarSlots.Count) return;
        var slot = hotbarSlots[selectedIndex];
        int newCount = slot.GetCount() - amount;

        if (newCount <= 0) slot.ClearSlot();
        else slot.SetSlot(slot.GetItem(), slot.GetAbility(), newCount, slot.GetDurability());

        ForceSyncAndSave();
    }

    public ItemData GetSelectedItem() => (selectedIndex >= 0 && selectedIndex < hotbarSlots.Count) ? hotbarSlots[selectedIndex].GetItem() : null;
    private void NotifySelectionChanged() => OnSelectedItemChanged?.Invoke(GetSelectedItem());

    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null) return;
        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < data.Count)
                hotbarSlots[i].SetSlot(data[i].item, data[i].ability, data[i].count, data[i].currentDurability);
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
            while (data.Count <= i) data.Add(new HotbarSlotData());
            data[i].item = hotbarSlots[i].GetItem();
            data[i].ability = hotbarSlots[i].GetAbility();
            data[i].count = hotbarSlots[i].GetCount();
            data[i].currentDurability = hotbarSlots[i].GetDurability();
        }
    }

    public void ForceSyncAndSave()
    {
        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }
}