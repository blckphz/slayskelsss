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
        {
            ConsumeFuel();
        }
    }

    public void InitializeFromSave()
    {
        lastFuelInt = Mathf.CeilToInt(fuelAmount);
        UpdateVisuals();
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
                NotifyUI();
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

    public int RemoveFuel(int amount)
    {
        if (fuelAmount < 1f) return 0;

        int available = Mathf.FloorToInt(fuelAmount);
        int toRemove = Mathf.Min(amount, available);

        fuelAmount -= toRemove;
        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        if (fuelAmount <= 0) Extinguish();

        NotifyUI();
        BuildingSaveManager.Instance?.SaveNow();
        return toRemove;
    }

    public void Ignite()
    {
        if (fuelAmount > 0)
        {
            isBurning = true;
            UpdateVisuals();
            NotifyUI();
            BuildingSaveManager.Instance?.SaveNow();
        }
    }

    public void Extinguish()
    {
        isBurning = false;
        UpdateVisuals();
        NotifyUI();
        BuildingSaveManager.Instance?.SaveNow();
    }

    public int AddFuel(ItemData item, int amount)
    {
        if (fuelItem == null) fuelItem = item;
        if (fuelItem.itemID != item.itemID) return 0;

        float spaceLeft = maxFuel - fuelAmount;
        int amountToAdd = Mathf.Min((int)spaceLeft, amount);

        fuelAmount += amountToAdd;
        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        NotifyUI();
        return amountToAdd;
    }

    private void NotifyUI()
    {
        if (CampfireUI.Instance != null && CampfireUI.Instance.CurrentCampfire == this)
        {
            CampfireUI.Instance.RefreshUI();
        }
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