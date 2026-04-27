using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Useable Item")]
public class UseableItem : ItemData
{
    [Header("Ability to Trigger")]
    public Ability abilityToExecute;

    public override void Use(Transform caster, Transform targetAnchor)
    {
        // =========================
        // 1. SAFETY CHECKS
        // =========================
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("<color=red>[Item System]</color> InventoryManager missing!");
            return;
        }

        if (abilityToExecute == null)
        {
            Debug.LogWarning($"<color=yellow>[Item System]</color> {itemName} has no Ability assigned!");
            return;
        }

        // =========================
        // 2. CHECK IF ITEM EXISTS (DEBUG PURPOSE ONLY)
        // =========================
        int currentCount = GetCurrentCount();

        if (currentCount <= 0)
        {
            Debug.Log($"<color=red>[Item System]</color> Cannot use {itemName} (ID: {itemID}). Count is 0!");
            return;
        }

        // =========================
        // 3. EXECUTE ABILITY
        // =========================

        bool hasExecuted = abilityToExecute.Execute(caster, targetAnchor, true);

        // =========================
        // 4. RESULT HANDLING
        // =========================
        if (hasExecuted)
        {
            // ❌ IMPORTANT FIX:
            // We DO NOT remove items here anymore.
            // HotbarManager is the ONLY system allowed to modify stack counts.

            int remaining = GetCurrentCount();

        }
        else
        {
            Debug.Log("<color=orange>[Item System]</color> Ability failed to execute. Item not consumed.");
        }
    }

    // =========================
    // GLOBAL COUNT CHECK (READ ONLY)
    // =========================
    private int GetCurrentCount()
    {
        if (InventoryManager.Instance == null) return 0;

        int totalFound = 0;

        // Check Inventory
        foreach (var slot in InventoryManager.Instance.inventory)
        {
            if (slot.item != null && slot.item.itemID == this.itemID)
                totalFound += slot.count;
        }

        // Check Hotbar
        foreach (var hSlot in InventoryManager.Instance.hotbarData)
        {
            if (hSlot.item != null && hSlot.item.itemID == this.itemID)
                totalFound += hSlot.count;
        }

        return totalFound;
    }
}