using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbarManager : MonoBehaviour
{
    public static PlayerHotbarManager Instance;

    [Header("UI References")]
    public List<InventorySlotUI> hotbarSlots = new List<InventorySlotUI>();
    public RectTransform selector;

    [Header("Combat Context")]
    public Transform caster;
    public Transform targetAnchor;

    private int selectedIndex = 0;
    private PlayerControls controls;

    private void Awake()
    {
        Instance = this;
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Use.performed += ctx => ExecuteActiveSlot();
        controls.Player.UseHotkey.performed += ctx => ExecuteActiveSlot();
        controls.Player.Scroll.performed += ctx => HandleScroll(ctx.ReadValue<Vector2>());
    }

    private void OnDisable() => controls.Disable();

    private void Update()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame) SelectSlot(i);
        }
    }

    private void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, hotbarSlots.Count - 1);
        UpdateSelector();
    }

    private void HandleScroll(Vector2 scrollVector)
    {
        if (scrollVector.y > 0) selectedIndex = (selectedIndex - 1 + 9) % 9;
        else if (scrollVector.y < 0) selectedIndex = (selectedIndex + 1) % 9;
        UpdateSelector();
    }

    public void UpdateSelector()
    {
        if (selector == null || hotbarSlots.Count <= selectedIndex) return;
        selector.position = hotbarSlots[selectedIndex].transform.position;
    }

    // 🔥 NEW: Check if an item or ability already exists in any hotbar slot
    public bool IsAlreadyInHotbar(ItemData item, Ability ability, InventorySlotUI excludingSlot)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot == excludingSlot) continue; // Don't check the slot we are currently dropping into

            if (item != null && slot.GetItem() == item) return true;
            if (ability != null && slot.GetAbility() == ability) return true;
        }
        return false;
    }
    public void ExecuteActiveSlot()
    {
        if (hotbarSlots.Count <= selectedIndex) return;
        InventorySlotUI slot = hotbarSlots[selectedIndex];

        // Check for Ability directly in slot (if it's a class ability)
        if (slot.GetAbility() != null)
        {
            slot.GetAbility().Execute(caster, targetAnchor, true);
        }
        // Check for Item (which will now trigger its assigned ability)
        else if (slot.GetItem() != null)
        {
            slot.GetItem().Use(caster, targetAnchor);
        }
    }



    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null) return;
        var dataList = InventoryManager.Instance.hotbarData;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            // Ensure data list is long enough
            while (dataList.Count <= i) dataList.Add(new HotbarSlotData());

            dataList[i].item = hotbarSlots[i].GetItem();
            dataList[i].ability = hotbarSlots[i].GetAbility();
            dataList[i].count = hotbarSlots[i].GetCount(); // 🔥 Transfer count to background data
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
                hotbarSlots[i].SetSlot(data[i].item, data[i].ability, data[i].count); // 🔥 Set UI count from data
            else
                hotbarSlots[i].ClearSlot();
        }
    }
}