using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controls = new PlayerControls();
    }

    private IEnumerator Start()
    {
        selectedIndex = 0;
        // Wait for Unity UI to fully layout slots
        yield return null;

        RefreshHotbar();
        UpdateSelector();
    }

    private void OnEnable()
    {
        controls.Enable();

        // MOUSE CLICK REMOVED: 
        // We no longer bind controls.Player.Use.performed here.

        controls.Player.Scroll.performed += ctx => HandleScroll(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // 1. Numeric hotkeys (1-9) for selecting slots
        for (int i = 0; i < hotbarSlots.Count && i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                SelectSlot(i);
        }

        // 2. USE ITEM: Only triggered by 'E' key
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteActiveSlot();
        }
    }

    // ---------------- SLOT SELECTION ----------------

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbarSlots.Count) return;

        selectedIndex = index;
        UpdateSelector();
    }

    private void HandleScroll(Vector2 scrollVector)
    {
        int count = hotbarSlots.Count;
        if (count == 0) return;

        if (scrollVector.y > 0)
            selectedIndex = (selectedIndex - 1 + count) % count;
        else if (scrollVector.y < 0)
            selectedIndex = (selectedIndex + 1) % count;

        UpdateSelector();
    }

    public void UpdateSelector()
    {
        if (selector == null || hotbarSlots.Count <= selectedIndex) return;

        selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    // ---------------- DATA ACCESS ----------------

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

    // ---------------- EXECUTION & COOLDOWN ----------------

    public void ExecuteActiveSlot()
    {
        // 1. Basic Validation
        if (hotbarSlots.Count <= selectedIndex) return;

        // 2. Cooldown Check: Respects the fireRate of the item/ability
        if (Time.time < nextFireTime)
        {
            Debug.Log("<color=orange>Ability on Cooldown!</color>");
            return;
        }

        InventorySlotUI slot = hotbarSlots[selectedIndex];

        // 3. Execution Logic
        if (slot.GetAbility() != null)
        {
            // Direct Ability Slot
            Ability ability = slot.GetAbility();
            bool success = ability.Execute(caster, targetAnchor, true);

            if (success)
            {
                SetCooldown(ability.fireRate);
            }
        }
        else if (slot.GetItem() != null)
        {
            ItemData item = slot.GetItem();

            if (!(item is buildSO))
            {
                // Pull cooldown from the item's ability
                float cooldownToApply = 0.2f;
                if (item is UseableItem useable && useable.abilityToExecute != null)
                {
                    cooldownToApply = useable.abilityToExecute.fireRate;
                }

                // Use() triggers the ability and item consumption
                item.Use(caster, targetAnchor);

                // Start cooldown
                SetCooldown(cooldownToApply);
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
        {
            slot.ClearSlot();
        }

        SyncHotbarToData();
    }

    // ---------------- DATA SYNCING ----------------

    public bool IsAlreadyInHotbar(ItemData item, Ability ability, InventorySlotUI excludingSlot)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot == excludingSlot) continue;
            if (item != null && slot.GetItem() == item) return true;
            if (ability != null && slot.GetAbility() == ability) return true;
        }
        return false;
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
}