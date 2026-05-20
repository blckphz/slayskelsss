using System;
using System.Collections;
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
        yield return null;

        selectedIndex = 0;

        Debug.Log("[HOTBAR INIT] First refresh");
        RefreshHotbar();

        UpdateSelector();
        NotifySelectionChanged();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Player.PrimaryItemUse.performed += _ => ExecuteActiveSlot();
        controls.Player.SecondaryItemUse.performed += _ => ExecuteSecondaryActiveSlot();
        controls.Player.Scroll.performed += ctx => HandleScroll(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
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

    // =========================
    // EXECUTE SLOT
    // =========================
    public void ExecuteActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count)
        {
            Debug.LogWarning("[HOTBAR] Invalid selected index");
            return;
        }

        if (Time.time < nextFireTime) return;

        var slot = hotbarSlots[selectedIndex];

        Debug.Log($"[HOTBAR USE] Slot {selectedIndex} item={slot.GetItem()?.name ?? "NULL"}");

        Ability ability = slot.GetAbility();

        if (ability != null)
        {
            if (ability.isOnCooldown) return;

            if (ability.Execute(caster, targetAnchor, true))
            {
                nextFireTime = Time.time + ability.fireRate;
                StartCoroutine(CooldownRoutine(ability));
            }

            return;
        }

        ItemData item = slot.GetItem();

        if (item == null || slot.GetCount() <= 0)
        {
            Debug.LogWarning("[HOTBAR USE] Empty slot");
            return;
        }

        float cooldown = 0.5f;

        if (item is UseableItem useable)
        {
            cooldown = useable.useRate;

            if (useable.abilityToExecute != null)
            {
                Debug.Log($"[HOTBAR USE] Trigger ability from item {item.name}");
                useable.abilityToExecute.Execute(caster, targetAnchor, true);

                nextFireTime = Time.time + cooldown;
                return;
            }
        }

        UseSelectedStack(item.consumeAmount);
        nextFireTime = Time.time + cooldown;
    }

    private IEnumerator CooldownRoutine(Ability ability)
    {
        ability.isOnCooldown = true;
        yield return new WaitForSeconds(ability.fireRate);
        ability.isOnCooldown = false;
    }

    // =========================
    // SECONDARY USE
    // =========================
    public void ExecuteSecondaryActiveSlot()
    {
        if (selectedIndex >= hotbarSlots.Count) return;

        var slot = hotbarSlots[selectedIndex];
        ItemData item = slot.GetItem();

        if (item == null || !item.isUsable) return;

        Debug.Log($"[HOTBAR SECONDARY] Slot {selectedIndex} item={item.name}");

        if (item is UseableItem useable && useable.abilityToExecute != null)
        {
            useable.abilityToExecute.ExecuteSecondary(caster);
        }
    }

    // =========================
    // USE STACK
    // =========================
    public void UseSelectedStack(int amount)
    {
        var slot = hotbarSlots[selectedIndex];

        int newCount = slot.GetCount() - amount;

        Debug.Log($"[HOTBAR CONSUME] Slot {selectedIndex} -{amount} => {newCount}");

        if (newCount <= 0)
        {
            Debug.Log("[HOTBAR CONSUME] Clearing slot");
            slot.ClearSlot();
        }
        else
        {
            slot.SetSlot(slot.GetItem(), slot.GetAbility(), newCount, slot.GetDurability());
        }

        ForceSyncAndSave();
    }

    // =========================
    // SELECT SLOT
    // =========================
    private void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1);

        Debug.Log($"[HOTBAR SELECT] Slot {selectedIndex}");

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

        Debug.Log($"[HOTBAR SCROLL] Selected {selectedIndex}");

        UpdateSelector();
        NotifySelectionChanged();
    }

    private void UpdateSelector()
    {
        if (selector != null && selectedIndex < hotbarSlots.Count)
            selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    // =========================
    // REFRESH FROM DATA
    // =========================
    public void RefreshHotbar()
    {
        var data = InventoryManager.Instance?.hotbarData;
        if (data == null)
        {
            Debug.LogWarning("[HOTBAR REFRESH] No data found");
            return;
        }

        Debug.Log("[HOTBAR REFRESH] Loading from save data");

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < data.Count)
            {
                Debug.Log($"[HOTBAR LOAD {i}] {data[i].item?.name ?? "NULL"} x{data[i].count}");

                hotbarSlots[i].SetSlot(
                    data[i].item,
                    data[i].ability,
                    data[i].count,
                    data[i].currentDurability
                );
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
        }

        UpdateSelector();
    }

    // =========================
    // SAVE TO DATA
    // =========================
    public void SyncHotbarToData()
    {
        var data = InventoryManager.Instance?.hotbarData;
        if (data == null)
        {
            Debug.LogWarning("[HOTBAR SAVE] No save data container");
            return;
        }

        Debug.Log("[HOTBAR SAVE] Writing hotbar -> data");

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            while (data.Count <= i)
                data.Add(new HotbarSlotData());

            var slot = hotbarSlots[i];

            data[i].item = slot.GetItem();
            data[i].ability = slot.GetAbility();
            data[i].count = slot.GetCount();
            data[i].currentDurability = slot.GetDurability();

            Debug.Log($"[HOTBAR SAVE {i}] item={data[i].item?.name ?? "NULL"} x{data[i].count}");
        }
    }

    public void ForceSyncAndSave()
    {
        Debug.Log("[HOTBAR SAVE] Force sync + save triggered");

        SyncHotbarToData();
        InventoryManager.Instance?.SaveInventory();
    }

    // =========================
    // EVENTS
    // =========================
    private void NotifySelectionChanged()
    {
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

    public ItemData GetSelectedItem()
    {
        return (selectedIndex < hotbarSlots.Count)
            ? hotbarSlots[selectedIndex].GetItem()
            : null;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }
}