using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbarManager : MonoBehaviour
{
    public static PlayerHotbarManager Instance;

    [Header("UI References")]
    public List<InventorySlotUI> hotbarSlots = new List<InventorySlotUI>();
    public RectTransform selector;

    [Header("Combat & Interaction Context")]
    public Transform caster;
    public Transform targetAnchor;

    [Header("Cooldown State")]
    private float nextFireTime = 0f;

    private int selectedIndex = 0;
    private PlayerControls controls;

    // =========================
    // EVENT (NEW SYSTEM)
    // =========================
    public event Action<ItemData> OnSelectedItemChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        controls = new PlayerControls();
    }

    private IEnumerator Start()
    {
        selectedIndex = 0;
        yield return null;

        RefreshHotbar();   // ✅ RESTORED (fix error)
        UpdateSelector();

        NotifySelectionChanged();
    }

    private void OnEnable()
    {
        controls.Enable();
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

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteActiveSlot(); // ✅ RESTORED
        }
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbarSlots.Count) return;

        if (selectedIndex == index) return;

        selectedIndex = index;
        UpdateSelector();

        NotifySelectionChanged();
    }

    private void HandleScroll(Vector2 scrollVector)
    {
        int count = hotbarSlots.Count;
        if (count == 0) return;

        int oldIndex = selectedIndex;

        if (scrollVector.y > 0)
            selectedIndex = (selectedIndex - 1 + count) % count;
        else if (scrollVector.y < 0)
            selectedIndex = (selectedIndex + 1) % count;

        if (oldIndex != selectedIndex)
        {
            UpdateSelector();
            NotifySelectionChanged();
        }
    }

    // =========================
    // EVENT DISPATCH
    // =========================
    private void NotifySelectionChanged()
    {
        OnSelectedItemChanged?.Invoke(GetSelectedItem());
    }

    public ItemData GetSelectedItem()
    {
        if (hotbarSlots.Count <= selectedIndex) return null;
        return hotbarSlots[selectedIndex].GetItem();
    }

    public int GetSelectedCount()
    {
        if (hotbarSlots.Count <= selectedIndex) return 0;
        return hotbarSlots[selectedIndex].GetCount();
    }

    public void UpdateSelector()
    {
        if (selector == null || hotbarSlots.Count <= selectedIndex) return;
        selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    // ======================================================
    // 🔧 LEGACY FUNCTIONS (RESTORED FOR COMPILATION SAFETY)
    // ======================================================

    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null) return;

        var data = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < data.Count)
                hotbarSlots[i].SetSlot(data[i].item, data[i].ability, data[i].count);
            else
                hotbarSlots[i].ClearSlot();
        }
    }

    public void ExecuteActiveSlot()
    {
        if (hotbarSlots.Count <= selectedIndex) return;

        if (Time.time < nextFireTime)
            return;

        InventorySlotUI slot = hotbarSlots[selectedIndex];

        if (slot.GetAbility() != null)
        {
            Ability ability = slot.GetAbility();
            bool success = ability.Execute(caster, targetAnchor, true);
            if (success) SetCooldown(ability.fireRate);
        }
        else if (slot.GetItem() != null)
        {
            ItemData item = slot.GetItem();

            if (!(item is buildSO))
            {
                float cooldown = 0.2f;

                if (item is UseableItem useable && useable.abilityToExecute != null)
                    cooldown = useable.abilityToExecute.fireRate;

                item.Use(caster, targetAnchor);
                SetCooldown(cooldown);
            }
        }
    }

    private void SetCooldown(float duration)
    {
        nextFireTime = Time.time + duration;
    }

    public void UseSelectedStack(int amount)
    {
        if (hotbarSlots.Count <= selectedIndex) return;

        InventorySlotUI slot = hotbarSlots[selectedIndex];

        slot.UpdateCount(-amount);

        if (slot.GetCount() <= 0)
            slot.ClearSlot();

        SyncHotbarToData();
    }

    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null) return;

        var dataList = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            while (dataList.Count <= i)
                dataList.Add(new HotbarSlotData());

            dataList[i].item = hotbarSlots[i].GetItem();
            dataList[i].ability = hotbarSlots[i].GetAbility();
            dataList[i].count = hotbarSlots[i].GetCount();
        }

        InventoryManager.Instance.SaveInventory();
    }
}