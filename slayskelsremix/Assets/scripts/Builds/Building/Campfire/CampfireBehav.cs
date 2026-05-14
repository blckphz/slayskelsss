using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CampfireBehav : MonoBehaviour, IInteractable, IBuildPreview
{
    public bool isBurning;

    [Header("Fuel")]
    public ItemData fuelItem;

    // ✅ NEW: default fuel type assigned in prefab (Wood)
    public ItemData defaultFuelItem;

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

        // ✅ SAFE FIX: ensure fuel type exists even after load/spawn
        if (fuelItem == null && defaultFuelItem != null)
        {
            fuelItem = defaultFuelItem;
            Debug.Log($"[Campfire] Assigned default fuel: {fuelItem.itemName}");
        }
    }

    private void Start()
    {
        lastFuelInt = Mathf.CeilToInt(fuelAmount);
        UpdateVisuals();

        Debug.Log(
            $"[Campfire] Start() fuelItem={(fuelItem ? fuelItem.itemName : "NULL")} " +
            $"fuelItemID={(fuelItem ? fuelItem.itemID : -1)}"
        );

        fuelItem = defaultFuelItem;

    }

    private void Update()
    {
        if (isBurning)
            ConsumeFuel();
    }

    // ---------------- NPC INTERACTION ----------------
    public void NPCInteract(NpcInvBrain npcInv)
    {
        if (fuelItem == null)
        {
            Debug.LogWarning("[Campfire] fuelItem is NULL, NPC cannot interact");
            return;
        }

        var woodSlot = npcInv.inventory.Find(
            slot => slot.item != null &&
                    slot.item.itemID == fuelItem.itemID
        );

        if (woodSlot == null || woodSlot.count <= 0)
            return;

        int canAccept = Mathf.Min(
            woodSlot.count,
            Mathf.FloorToInt(maxFuel - fuelAmount)
        );

        if (canAccept <= 0)
            return;

        AddFuel(woodSlot.item, canAccept);

        woodSlot.count -= canAccept;

        if (woodSlot.count <= 0)
            npcInv.inventory.Remove(woodSlot);

        if (!isBurning)
            Ignite();
    }

    // ---------------- FUEL CONSUMPTION ----------------
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
                Extinguish();
            }
        }
        else
        {
            Extinguish();
        }
    }

    // ---------------- ADD FUEL ----------------
    public int AddFuel(ItemData item, int amount)
    {
        if (fuelItem == null)
            fuelItem = item;

        if (fuelItem.itemID != item.itemID)
            return 0;

        int amountToAdd = Mathf.Min(
            Mathf.FloorToInt(maxFuel - fuelAmount),
            amount
        );

        fuelAmount += amountToAdd;
        lastFuelInt = Mathf.CeilToInt(fuelAmount);

        UpdateVisuals();
        CampfireUI.Instance?.RefreshUI();
        BuildingSaveManager.Instance?.SaveAfterChange();

        return amountToAdd;
    }

    // ---------------- REMOVE FUEL ----------------
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
            Extinguish();
        }

        UpdateVisuals();
        CampfireUI.Instance?.RefreshUI();
        BuildingSaveManager.Instance?.SaveAfterChange();

        return toRemove;
    }

    // ---------------- IGNITE / EXTINGUISH ----------------
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
        if (!isBurning)
            return;

        isBurning = false;
        UpdateVisuals();
    }

    // ---------------- VISUALS ----------------
    private void UpdateVisuals()
    {
        if (anim != null)
            anim.SetBool("Burning", isBurning);

        if (fireLight != null)
            fireLight.enabled = isBurning;
    }

    // ---------------- INTERACTION ----------------
    public void Interact(InventoryManager playerInventory)
    {
        CampfireUI.Instance?.OpenCampfire(this);
    }

    public void OnFocus()
    {
        highlight?.SetHighlighted(true);
    }

    public void OnLoseFocus()
    {
        highlight?.SetHighlighted(false);
    }

    public string GetPrompt()
    {
        return isBurning ? "Manage Campfire" : "Light Campfire";
    }

    public void OnPreviewUpdate() { }
}