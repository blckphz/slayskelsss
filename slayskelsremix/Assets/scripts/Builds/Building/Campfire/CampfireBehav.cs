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

    // =========================
    // SAVE HELPER (IMPORTANT)
    // =========================
    private void Save()
    {
        BuildingSaveManager.Instance?.SaveAfterChange();
    }

    // =========================
    // FUEL CONSUMPTION
    // =========================
    private void ConsumeFuel()
    {
        if (fuelAmount > 0)
        {
            fuelAmount -= burnRate * Time.deltaTime;

            int currentFuelInt = Mathf.CeilToInt(fuelAmount);
            if (currentFuelInt != lastFuelInt)
            {
                lastFuelInt = currentFuelInt;
            }

            if (fuelAmount <= 0)
            {
                fuelAmount = 0;
                Debug.Log($"<color=orange>[Campfire]</color> {gameObject.name} ran out of fuel.");
                Extinguish();
            }
        }
        else
        {
            Extinguish();
        }
    }

    // =========================
    // ADD FUEL
    // =========================
    public int AddFuel(ItemData item, int amount)
    {
        if (fuelItem == null)
            fuelItem = item;

        if (fuelItem.itemID != item.itemID)
            return 0;

        int amountToAdd = Mathf.Min((int)(maxFuel - fuelAmount), amount);
        fuelAmount += amountToAdd;

        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        Debug.Log($"<color=orange>[Campfire]</color> Added {amountToAdd} fuel. Total: {fuelAmount}");

        UpdateVisuals();
        Save(); // 🔥 SAVE HERE

        return amountToAdd;
    }

    // =========================
    // REMOVE FUEL
    // =========================
    public int RemoveFuel(int amount)
    {
        if (fuelAmount < 1f) return 0;

        int toRemove = Mathf.Min(amount, Mathf.FloorToInt(fuelAmount));
        fuelAmount -= toRemove;

        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        Debug.Log($"<color=orange>[Campfire]</color> Removed {toRemove} fuel. Left: {fuelAmount}");

        if (fuelAmount <= 0)
            Extinguish();

        UpdateVisuals();
        Save(); // 🔥 SAVE HERE

        return toRemove;
    }

    // =========================
    // IGNITE
    // =========================
    public void Ignite()
    {
        if (fuelAmount > 0)
        {
            isBurning = true;

            Debug.Log($"<color=orange>[Campfire]</color> Ignited.");

            UpdateVisuals();
            Save(); // 🔥 SAVE HERE
        }
    }

    // =========================
    // EXTINGUISH
    // =========================
    public void Extinguish()
    {
        if (!isBurning) return;

        isBurning = false;

        Debug.Log($"<color=orange>[Campfire]</color> Extinguished.");

        UpdateVisuals();
        Save(); // 🔥 SAVE HERE
    }

    // =========================
    // VISUALS
    // =========================
    private void UpdateVisuals()
    {
        if (anim != null)
            anim.SetBool("Burning", isBurning);

        if (fireLight != null)
            fireLight.enabled = isBurning;
    }

    // =========================
    // INTERACTION
    // =========================
    public void Interact(InventoryManager playerInventory)
        => CampfireUI.Instance?.OpenCampfire(this);

    public void OnFocus()
        => highlight?.SetHighlighted(true);

    public void OnLoseFocus()
        => highlight?.SetHighlighted(false);

    public string GetPrompt()
        => isBurning ? "Manage Campfire" : "Light Campfire";

    public void OnPreviewUpdate() { }
}