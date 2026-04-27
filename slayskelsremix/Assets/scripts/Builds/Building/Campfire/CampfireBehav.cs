using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CampfireBehav : MonoBehaviour, IInteractable, IBuildPreview
{
    public bool isBurning;

    [Header("Fuel")]
    public ItemData fuelItem;
    public float fuelAmount;
    public int maxFuel = 10;
    public float burnRate = 0.5f;

    [Header("Effects")]
    public Light2D fireLight;

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
    }

    private void Update()
    {
        if (isBurning)
            ConsumeFuel();
    }

    private void Save()
    {
        BuildingSaveManager.Instance?.SaveAfterChange();
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
                // Update UI text every time the integer count drops
                CampfireUI.Instance?.RefreshUI();
            }

            if (fuelAmount <= 0)
            {
                fuelAmount = 0;
                Extinguish();
            }
        }
        else
        {
            Extinguish();
        }
    }

    public int AddFuel(ItemData item, int amount)
    {
        // Set the reference immediately before checks
        if (fuelItem == null) fuelItem = item;

        if (fuelItem.itemID != item.itemID)
        {
            Debug.LogWarning($"<color=orange>[Campfire]</color> Item ID mismatch! Expected {fuelItem.itemID}, got {item.itemID}");
            return 0;
        }

        int amountToAdd = Mathf.Min((int)(maxFuel - fuelAmount), amount);
        fuelAmount += amountToAdd;
        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        Debug.Log($"<color=orange>[Campfire]</color> Added {amountToAdd}. New Total: {fuelAmount}");

        UpdateVisuals();

        // FORCE UI REFRESH
        CampfireUI.Instance?.RefreshUI();
        Save();

        return amountToAdd;
    }

    public int RemoveFuel(int amount)
    {
        if (fuelAmount < 1f) return 0;

        int toRemove = Mathf.Min(amount, Mathf.FloorToInt(fuelAmount));
        fuelAmount -= toRemove;
        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        Debug.Log($"<color=orange>[Campfire]</color> Removed {toRemove}. Remaining: {fuelAmount}");

        if (fuelAmount <= 0)
        {
            fuelAmount = 0;
            fuelItem = null; // Clear item reference if empty
            Extinguish();
        }

        UpdateVisuals();

        // FORCE UI REFRESH
        CampfireUI.Instance?.RefreshUI();
        Save();

        return toRemove;
    }

    public void Ignite()
    {
        if (fuelAmount > 0)
        {
            isBurning = true;
            Debug.Log("<color=orange>[Campfire]</color> Ignited.");
            UpdateVisuals();
            Save();
        }
    }

    public void Extinguish()
    {
        if (!isBurning) return;
        isBurning = false;
        Debug.Log("<color=orange>[Campfire]</color> Extinguished.");
        UpdateVisuals();
        Save();
    }

    private void UpdateVisuals()
    {
        if (anim != null) anim.SetBool("Burning", isBurning);
        if (fireLight != null) fireLight.enabled = isBurning;
    }

    public void Interact(InventoryManager playerInventory) => CampfireUI.Instance?.OpenCampfire(this);
    public void OnFocus() => highlight?.SetHighlighted(true);
    public void OnLoseFocus() => highlight?.SetHighlighted(false);
    public string GetPrompt() => isBurning ? "Manage Campfire" : "Light Campfire";
    public void OnPreviewUpdate() { }
}