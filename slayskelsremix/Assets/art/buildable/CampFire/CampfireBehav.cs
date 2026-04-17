using UnityEngine;

public class CampfireBehav : MonoBehaviour, IInteractable, IBuildPreview
{
    public bool isBurning;

    [Header("Fuel")]
    public ItemData fuelItem;
    public int fuelAmount;
    public int maxFuel = 10;

    private Animator anim;
    private Highlightable highlight;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        highlight = GetComponent<Highlightable>();
    }

    public void OnFocus()
    {
        highlight?.SetHighlighted(true);
    }

    public void OnLoseFocus()
    {
        highlight?.SetHighlighted(false);
        CampfireUI.Instance?.CloseCampfire();
    }

    public void Interact(InventoryManager playerInventory)
    {
        CampfireUI.Instance?.OpenCampfire(this);
    }

    // Only describes the action (NOT the key anymore)
    public string GetPrompt()
    {
        return "Use Campfire";
    }

    public void Ignite()
    {
        isBurning = true;

        if (anim != null)
            anim.Play("Burning");
    }

    public int AddFuel(ItemData item, int amount)
    {
        if (fuelItem != null && fuelItem.itemID != item.itemID)
            return 0;

        if (fuelItem == null)
            fuelItem = item;

        int spaceLeft = maxFuel - fuelAmount;
        int amountToAdd = Mathf.Min(spaceLeft, amount);

        fuelAmount += amountToAdd;

        return amountToAdd;
    }

    public void OnPreviewUpdate() { }
}