using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerHotbarManager : MonoBehaviour
{
    public static PlayerHotbarManager Instance;

    [Header("Hotbar")]
    public List<InventorySlotUI> hotbarSlots =
        new List<InventorySlotUI>();

    public RectTransform selector;

    [Header("References")]
    public Transform caster;
    public Transform targetAnchor;

    public int selectedIndex = 0;

    private PlayerControls controls;

    public event Action<ItemData> OnSelectedItemChanged;

    private bool primaryQueued;
    private bool secondaryQueued;

    private float lastBuildModeMessageTime;

    private const float buildModeMessageCooldown = 1f;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        controls = new PlayerControls();
    }


    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerControls();
        }

        controls.Enable();

        controls.Player.PrimaryItemUse.performed +=
            OnPrimaryUse;

        controls.Player.SecondaryItemUse.performed +=
            OnSecondaryUse;

        controls.Player.Scroll.performed +=
            OnScroll;
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (controls == null)
        {
            return;
        }

        controls.Player.PrimaryItemUse.performed -=
            OnPrimaryUse;

        controls.Player.SecondaryItemUse.performed -=
            OnSecondaryUse;

        controls.Player.Scroll.performed -=
            OnScroll;

        controls.Disable();
    }


    // =========================================================
    // INPUT CALLBACKS
    // =========================================================

    private void OnPrimaryUse(
        InputAction.CallbackContext ctx)
    {
        primaryQueued = true;
    }


    private void OnSecondaryUse(
        InputAction.CallbackContext ctx)
    {
        secondaryQueued = true;
    }


    private void OnScroll(
        InputAction.CallbackContext ctx)
    {
        HandleScroll(
            ctx.ReadValue<Vector2>()
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        bool isPointerOverUI =
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();


        if (!isPointerOverUI)
        {
            if (primaryQueued &&
                !ActionLock.IsLocked)
            {
                primaryQueued = false;

                ExecuteActiveSlot();
            }


            if (secondaryQueued &&
                !ActionLock.IsLocked)
            {
                secondaryQueued = false;

                ExecuteSecondaryActiveSlot();
            }
        }
        else
        {
            primaryQueued = false;
            secondaryQueued = false;
        }


        // =====================================================
        // NUMBER KEYS
        // =====================================================

        for (
            int i = 0;
            i < hotbarSlots.Count && i < 9;
            i++
        )
        {
            if (Keyboard.current[
                    Key.Digit1 + i
                ].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }
    }


    // =========================================================
    // DURABILITY
    // =========================================================

    private bool UsesDurability(
        ItemData item)
    {
        return
            item != null &&
            item.maxDurability > 0;
    }


    // =========================================================
    // REFILL
    // =========================================================

    private void TryRefillSelectedSlot(
        ItemData previousItem)
    {
        if (previousItem == null)
        {
            return;
        }


        if (InventoryManager.Instance == null)
        {
            return;
        }


        var inventory =
            InventoryManager.Instance.inventory;


        for (int i = 0; i < inventory.Count; i++)
        {
            var invSlot = inventory[i];


            if (invSlot.item == null)
            {
                continue;
            }


            if (invSlot.item.itemID !=
                previousItem.itemID)
            {
                continue;
            }


            Ability ability =
                hotbarSlots[selectedIndex].GetAbility()
                ??
                (
                    previousItem is UseableItem u
                        ? u.abilityToExecute
                        : null
                );


            hotbarSlots[selectedIndex].SetSlot(
                invSlot.item,
                ability,
                invSlot.count,
                invSlot.currentDurability
            );


            invSlot.item = null;
            invSlot.count = 0;
            invSlot.currentDurability = 0;


            SyncHotbarToData();

            InventoryManager.Instance.RefreshAll();

            OnSelectedItemChanged?.Invoke(
                GetSelectedItem()
            );

            return;
        }
    }


    // =========================================================
    // CONSUME
    // =========================================================

    private void ConsumeFromHotbar(
        int amount)
    {
        var slot =
            hotbarSlots[selectedIndex];


        ItemData item =
            slot.GetItem();


        if (item == null)
        {
            return;
        }


        int newCount =
            slot.GetCount() - amount;


        if (newCount <= 0)
        {
            ItemData destroyedItem = item;

            slot.ClearSlot();

            TryRefillSelectedSlot(
                destroyedItem
            );
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


        SyncHotbarToData();
    }


    // =========================================================
    // SINGLE DURABILITY ITEM
    // =========================================================

    private void EnsureSingleItemInHotbar()
    {
        var slot =
            hotbarSlots[selectedIndex];


        ItemData item =
            slot.GetItem();


        if (item == null ||
            slot.GetCount() <= 1)
        {
            return;
        }


        int count =
            slot.GetCount();


        slot.SetSlot(
            item,
            slot.GetAbility(),
            1,
            slot.GetDurability()
        );


        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(
                item,
                count - 1,
                slot.GetDurability()
            );
        }
    }


    // =========================================================
    // PRIMARY ACTION
    // =========================================================

    public void ExecuteActiveSlot()
    {
        var slot =
            hotbarSlots[selectedIndex];


        ItemData item =
            slot.GetItem();


        if (item == null)
        {
            return;
        }


        // =====================================================
        // BUILD MODE
        // =====================================================

        if (BuildState.IsBuildMode)
        {
            if (
                Time.time -
                lastBuildModeMessageTime >
                buildModeMessageCooldown
            )
            {
                lastBuildModeMessageTime =
                    Time.time;
            }

            return;
        }


        // =====================================================
        // ABILITY
        // =====================================================

        Ability ability =
            slot.GetAbility()
            ??
            (
                item is UseableItem u
                    ? u.abilityToExecute
                    : null
            );


        if (ability == null)
        {
            return;
        }


        // =====================================================
        // DURABILITY
        // =====================================================

        if (UsesDurability(item))
        {
            EnsureSingleItemInHotbar();
        }


        // =====================================================
        // EXECUTE
        //
        // MELEE IS HANDLED HERE.
        // PLAYERATTACK IS FOR SPECIAL ABILITIES.
        // =====================================================

        bool success =
            ability.Execute(
                caster,
                targetAnchor,
                true
            );


        // =====================================================
        // CONSUME
        // =====================================================

        if (success &&
            item.isUsable)
        {
            ConsumeFromHotbar(
                item.consumeAmount
            );

            OnSelectedItemChanged?.Invoke(
                GetSelectedItem()
            );
        }
    }


    // =========================================================
    // SECONDARY ACTION
    // =========================================================

    private void ExecuteSecondaryActiveSlot()
    {
        var slot =
            hotbarSlots[selectedIndex];


        if (
            slot.GetItem() is UseableItem useable &&
            useable.abilityToExecute != null
        )
        {
            useable.abilityToExecute.ExecuteSecondary(
                caster
            );
        }
    }


    // =========================================================
    // DURABILITY GET
    // =========================================================

    public float GetActiveToolDurability()
    {
        return
            hotbarSlots[selectedIndex]
                ?.GetDurability()
            ?? 0f;
    }


    // =========================================================
    // DURABILITY REDUCE
    // =========================================================

    public void ReduceActiveToolDurability(
        int amount)
    {
        var slot =
            hotbarSlots[selectedIndex];


        ItemData item =
            slot.GetItem();


        if (
            item == null ||
            item.maxDurability <= 0
        )
        {
            return;
        }


        int currentDurability =
            slot.GetDurability() - amount;


        if (currentDurability <= 0)
        {
            ItemData destroyedItem =
                item;


            slot.ClearSlot();


            TryRefillSelectedSlot(
                destroyedItem
            );


            SyncHotbarToData();


            OnSelectedItemChanged?.Invoke(
                GetSelectedItem()
            );


            return;
        }


        slot.SetSlot(
            item,
            slot.GetAbility(),
            slot.GetCount(),
            currentDurability
        );


        SyncHotbarToData();
    }


    // =========================================================
    // REFRESH
    // =========================================================

    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }


        var data =
            InventoryManager.Instance.hotbarData;


        for (
            int i = 0;
            i < hotbarSlots.Count;
            i++
        )
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
    // SELECT SLOT
    // =========================================================

    public void SelectSlot(int index)
    {
        if (hotbarSlots.Count == 0)
        {
            return;
        }


        selectedIndex =
            Mathf.Clamp(
                index,
                0,
                hotbarSlots.Count - 1
            );


        UpdateSelector();


        OnSelectedItemChanged?.Invoke(
            GetSelectedItem()
        );
    }


    // =========================================================
    // SCROLL
    // =========================================================

    private void HandleScroll(
        Vector2 scroll)
    {
        if (hotbarSlots.Count == 0)
        {
            return;
        }


        if (scroll.y > 0)
        {
            selectedIndex =
                (
                    selectedIndex -
                    1 +
                    hotbarSlots.Count
                )
                % hotbarSlots.Count;
        }
        else if (scroll.y < 0)
        {
            selectedIndex =
                (
                    selectedIndex +
                    1
                )
                % hotbarSlots.Count;
        }


        UpdateSelector();


        OnSelectedItemChanged?.Invoke(
            GetSelectedItem()
        );
    }


    // =========================================================
    // SELECTOR
    // =========================================================

    private void UpdateSelector()
    {
        if (
            selector != null &&
            hotbarSlots.Count > 0
        )
        {
            selector.position =
                hotbarSlots[selectedIndex]
                    .transform.position;
        }
    }


    // =========================================================
    // GET SELECTED ITEM
    // =========================================================

    public ItemData GetSelectedItem()
    {
        if (hotbarSlots.Count == 0)
        {
            return null;
        }

        return
            hotbarSlots[selectedIndex]
                .GetItem();
    }


    // =========================================================
    // SYNC HOTBAR
    // =========================================================

    public void SyncHotbarToData()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }


        var data =
            InventoryManager.Instance.hotbarData;


        for (
            int i = 0;
            i < hotbarSlots.Count;
            i++
        )
        {
            if (i >= data.Count)
            {
                data.Add(
                    new HotbarSlotData()
                );
            }


            data[i].item =
                hotbarSlots[i].GetItem();


            data[i].ability =
                hotbarSlots[i].GetAbility();


            data[i].count =
                hotbarSlots[i].GetCount();


            data[i].currentDurability =
                hotbarSlots[i].GetDurability();
        }


        OnSelectedItemChanged?.Invoke(
            GetSelectedItem()
        );
    }


    // =========================================================
    // USE SELECTED STACK
    // =========================================================

    public void UseSelectedStack(
        int amount)
    {
        ConsumeFromHotbar(amount);
    }
}