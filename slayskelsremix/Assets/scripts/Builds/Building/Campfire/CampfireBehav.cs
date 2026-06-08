using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class CampfireBehav : MonoBehaviour, IInteractable, IBuildPreview
{
    public bool isBurning;

    [Header("Fuel System")]
    public List<ItemData> validFuelItems = new List<ItemData>();

    public float fuelAmount;
    public int maxFuel = 10;
    public float burnRate = 0.5f;

    [Header("Current Fuel Display")]
    public ItemData currentFuelItem;

    [Header("Effects")]
    public Light2D fireLight;

    public bool HasFuel => fuelAmount > 0f && currentFuelItem != null;

    public System.Action<bool> OnPulseStateChanged;

    private Animator anim;
    private Highlightable highlight;
    private int lastFuelInt;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        highlight = GetComponent<Highlightable>();
    }

    private void Start()
    {
        lastFuelInt = Mathf.CeilToInt(fuelAmount);
        UpdateVisuals();

        NotifyPulseState();
    }

    private void Update()
    {
        if (isBurning)
            ConsumeFuel();
    }

    private void NotifyPulseState()
    {
        OnPulseStateChanged?.Invoke(HasFuel);
    }

    public bool IsValidFuel(ItemData item)
    {
        if (item == null) return false;

        for (int i = 0; i < validFuelItems.Count; i++)
        {
            if (validFuelItems[i] != null &&
                validFuelItems[i].itemID == item.itemID)
                return true;
        }

        return false;
    }

    public void NPCInteract(NpcInvBrain npcInv)
    {
        var fuelSlot = npcInv.inventory.Find(slot =>
            slot.item != null && IsValidFuel(slot.item));

        if (fuelSlot == null || fuelSlot.count <= 0)
            return;

        int canAccept = Mathf.Min(
            fuelSlot.count,
            Mathf.FloorToInt(maxFuel - fuelAmount)
        );

        if (canAccept <= 0)
            return;

        AddFuel(fuelSlot.item, canAccept);

        fuelSlot.count -= canAccept;

        if (fuelSlot.count <= 0)
            npcInv.inventory.Remove(fuelSlot);

        if (!isBurning)
            Ignite();
    }

    private void ConsumeFuel()
    {
        if (fuelAmount > 0)
        {
            fuelAmount -= burnRate * Time.deltaTime;

            int currentFuelInt = Mathf.CeilToInt(fuelAmount);

            if (currentFuelInt != lastFuelInt)
            {
                lastFuelInt = currentFuelInt;
                CampfireUI.Instance?.RefreshUI();
            }

            if (fuelAmount <= 0)
            {
                fuelAmount = 0;
                currentFuelItem = null;

                Extinguish();
                NotifyPulseState();
            }
        }
        else
        {
            Extinguish();
            NotifyPulseState();
        }
    }

    public int AddFuel(ItemData item, int amount)
    {
        if (!IsValidFuel(item))
            return 0;

        int amountToAdd = Mathf.Min(
            Mathf.FloorToInt(maxFuel - fuelAmount),
            amount
        );

        if (amountToAdd <= 0)
            return 0;

        fuelAmount += amountToAdd;
        currentFuelItem = item;

        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        UpdateVisuals();
        CampfireUI.Instance?.RefreshUI();
        BuildingSaveManager.Instance?.SaveAfterChange();

        NotifyPulseState();

        return amountToAdd;
    }

    public int RemoveFuel(int amount)
    {
        if (fuelAmount < 1f)
            return 0;

        int toRemove = Mathf.Min(amount, Mathf.FloorToInt(fuelAmount));

        fuelAmount -= toRemove;
        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        if (fuelAmount <= 0)
        {
            fuelAmount = 0;
            currentFuelItem = null;

            Extinguish();
        }

        UpdateVisuals();
        CampfireUI.Instance?.RefreshUI();
        BuildingSaveManager.Instance?.SaveAfterChange();

        NotifyPulseState();

        return toRemove;
    }

    public void Ignite()
    {
        if (fuelAmount > 0)
        {
            isBurning = true;
            UpdateVisuals();
        }
    }

    public void Extinguish()
    {
        isBurning = false;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (anim != null)
            anim.SetBool("Burning", isBurning);

        if (fireLight != null)
            fireLight.enabled = isBurning;
    }

    public void Interact(InventoryManager playerInventory)
    {
        CampfireUI.Instance?.OpenCampfire(this);
    }

    public void OnFocus() => highlight?.SetHighlighted(true);

    public void OnLoseFocus() => highlight?.SetHighlighted(false);

    public string GetPrompt()
    {
        return isBurning ? "Manage Campfire" : "Light Campfire";
    }

    public void OnPreviewUpdate() { }
}