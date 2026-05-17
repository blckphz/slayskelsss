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

        // 💡 Safety Reset: Clear any leftover dirty data from previous editor play sessions
        foreach (var slot in hotbarSlots)
        {
            if (slot != null && slot.GetAbility() != null)
            {
                slot.GetAbility().isOnCooldown = false;
            }
        }

        Debug.Log("[Hotbar] Initialized and dirty states cleared.");
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

        Debug.Log("[Hotbar] Controls enabled");
    }

    private void OnDisable()
    {
        controls.Player.PrimaryItemUse.performed -= OnPrimaryUse;
        controls.Player.SecondaryItemUse.performed -= OnSecondaryUse;
        controls.Player.Scroll.performed -= OnScroll;

        controls.Disable();

        Debug.Log("[Hotbar] Controls disabled");
    }

    private void Update()
    {
        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                Debug.Log($"[Hotbar] Number key pressed -> Slot {i}");
                SelectSlot(i);
            }
        }
    }

    // ================= INPUT =================

    private void OnPrimaryUse(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Hotbar] Primary input triggered");
        ExecuteActiveSlot();
    }

    private void OnSecondaryUse(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Hotbar] Secondary input triggered");
        ExecuteSecondaryActiveSlot();
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        Vector2 scroll = ctx.ReadValue<Vector2>();
        HandleScroll(scroll);
    }

    // ================= MAIN =================

    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count)
        {
            Debug.LogWarning("[Hotbar] Invalid slot index");
            return;
        }

        if (Time.time < nextFireTime)
        {
            Debug.Log("[Hotbar] Blocked by cooldown");
            return;
        }

        var slot = hotbarSlots[selectedIndex];
        Debug.Log($"[Hotbar] Using slot {selectedIndex}");

        // CHECK 1: DIRECT ABILITY SLOTS (Spells/Skills)
        if (slot.GetAbility() != null)
        {
            Ability ability = slot.GetAbility();

            if (ability.isOnCooldown)
            {
                Debug.Log($"[Hotbar] {ability.abilityName} is still on cooldown.");
                return;
            }

            Debug.Log($"[Hotbar] Direct Ability found: {ability.name}");

            if (ability.Execute(caster, targetAnchor, true))
            {
                // Ranged/Standard abilities go straight to full fireRate cooldown
                nextFireTime = Time.time + ability.fireRate;
                StartCoroutine(AbilityCooldownRoutine(ability));

                Debug.Log($"[Hotbar] Direct Ability executed -> cooldown {ability.fireRate}");
            }
            else
            {
                Debug.Log("[Hotbar] Direct Ability did NOT execute");
            }
            return;
        }

        // CHECK 2: ITEM SLOTS (Tools / Consumables / Placeables)
        ItemData item = slot.GetItem();

        if (item == null)
        {
            Debug.LogWarning("[Hotbar] No item or ability found in slot");
            return;
        }

        if (!item.isUsable || slot.GetCount() <= 0)
        {
            Debug.LogWarning("[Hotbar] Item asset is not flagged as usable, or stack count is zero.");
            return;
        }

        if (item is UseableItem useableItem && useableItem.abilityToExecute != null)
        {
            Ability boundAbility = useableItem.abilityToExecute;

            if (boundAbility.isOnCooldown)
            {
                Debug.Log($"[Hotbar] Embedded ability {boundAbility.abilityName} is still on cooldown.");
                return;
            }

            Debug.Log($"[Hotbar] Item containing embedded action found: {item.name} -> Firing Ability: {boundAbility.name}");

            if (boundAbility.Execute(caster, targetAnchor, true))
            {
                if (boundAbility is offensivemelee melee)
                {
                    // 🎯 The time allowed between individual swings is determined by swingFreq
                    nextFireTime = Time.time + melee.swingFreq;

                    bool reachedComboEnd = (melee.maxSwings > 0 && typeof(offensivemelee)
                        .GetField("swingIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(melee) is int index && index == 0);

                    // 🎯 When the combo finishes, lock the weapon down for the length of fireRate
                    if (reachedComboEnd)
                    {
                        StartCoroutine(AbilityCooldownRoutine(boundAbility));
                        melee.ResetMeleeState();
                    }
                }
                else
                {
                    // Standard tools/weapons use fireRate directly for single-use recovery
                    nextFireTime = Time.time + boundAbility.fireRate;
                    StartCoroutine(AbilityCooldownRoutine(boundAbility));
                }

            }
            else
            {
                Debug.LogWarning("[Hotbar] Embedded tool execution failed or blocked by inner logic constraints.");
            }
            return;
        }

        // CHECK 3: CONSUMABLES (Items without an underlying ability asset)
        float defaultCooldown = item is UseableItem u ? u.useRate : 0.5f;


        UseSelectedStack(item.consumeAmount);
        nextFireTime = Time.time + defaultCooldown;
    }

    // 🎯 Cooldown length is strictly dependent on fireRate
    private IEnumerator AbilityCooldownRoutine(Ability ability)
    {
        ability.isOnCooldown = true;
        yield return new WaitForSeconds(ability.fireRate);
        ability.isOnCooldown = false;
    }

    // 🔥 Copy the same coroutine tracker helper down into the bottom of PlayerHotbarManager
    private IEnumerator AbilityCooldownRoutine(Ability ability, float duration)
    {
        ability.isOnCooldown = true;
        yield return new WaitForSeconds(duration);
        ability.isOnCooldown = false;
    }

    private void ExecuteSecondaryActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count)
            return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null || !item.isUsable)
        {
            Debug.Log("[Hotbar] Secondary failed: invalid item");
            return;
        }

        if (item is UseableItem useable && useable.abilityToExecute != null)
        {
            // 🔥 Check if the underlying ability is a Tool
            bool isTool = useable.abilityToExecute is ToolsSO || useable.abilityToExecute is offensivemelee;

            if (isTool)
            {
                Debug.Log($"<color=cyan>[Hotbar Secondary]</color> Tool detected: {useable.abilityToExecute.name}. BYPASSING cooldown checks completely.");
            }
            else if (Time.time < nextFireTime)
            {
                // Standard non-tool items still respect cooldown global limits
                Debug.Log($"[Hotbar Secondary] Non-tool item blocked by weapon/action cooldown. Remaining: {nextFireTime - Time.time:F2}s");
                return;
            }

            Debug.Log($"[Hotbar Secondary] Executing ability: {useable.abilityToExecute.name}");

            if (useable.abilityToExecute.ExecuteSecondary(caster))
            {
                // Tools bypass the global cooldown penalty entirely, normal items write to nextFireTime
                if (!isTool)
                {
                    nextFireTime = Time.time + useable.useRate;
                    Debug.Log($"[Hotbar Secondary] Normal secondary action registered. Global cooldown added: {useable.useRate}s");
                }
                else
                {
                    Debug.Log("[Hotbar Secondary] Tool secondary executed successfully without global cooldown penalties.");
                }
            }
        }
    }

    // ================= SLOT =================

    private void SelectSlot(int index)
    {
        if (index == selectedIndex)
        {
            Debug.Log($"[Hotbar] Slot {index} already selected");
            return;
        }

        selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1);
        Debug.Log($"[Hotbar] Selected slot -> {selectedIndex}");

        UpdateSelector();
        NotifySelectionChanged();
    }

    private void HandleScroll(Vector2 scroll)
    {
        if (hotbarSlots.Count == 0)
            return;

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

    // ================= ITEM =================

    public void UseSelectedStack(int amount)
    {
        Debug.Log($"[Hotbar] UseSelectedStack -> Amount: {amount}");

        if (selectedIndex >= hotbarSlots.Count)
        {
            Debug.LogWarning("[Hotbar] Invalid index in UseSelectedStack");
            return;
        }

        var slot = hotbarSlots[selectedIndex];
        Debug.Log($"[Hotbar] Before consume -> Count: {slot.GetCount()}");

        slot.UpdateCount(-amount);
        Debug.Log($"[Hotbar] After consume -> Count: {slot.GetCount()}");

        if (slot.GetCount() <= 0)
        {
            Debug.Log("[Hotbar] Slot empty -> clearing");
            slot.ClearSlot();
        }

        SyncHotbarToData();

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[Hotbar] InventoryManager is NULL");
        }
        else
        {
            Debug.Log("[Hotbar] Saving inventory");
            InventoryManager.Instance.SaveInventory();
        }
    }

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

    // ================= SYNC =================

    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[Hotbar] InventoryManager missing on Refresh");
            return;
        }

        var data = InventoryManager.Instance.hotbarData;
        Debug.Log("[Hotbar] Refreshing from save data");

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
        if (InventoryManager.Instance == null)
            return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            while (data.Count <= i)
                data.Add(new HotbarSlotData());

            data[i].item = hotbarSlots[i].GetItem();
            data[i].ability = hotbarSlots[i].GetAbility();
            data[i].count = hotbarSlots[i].GetCount();
        }

        Debug.Log("[Hotbar] Synced hotbar -> data");
    }

    public void ForceSyncAndSave()
    {
        Debug.Log("[Hotbar] Force sync + save");
        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }
}