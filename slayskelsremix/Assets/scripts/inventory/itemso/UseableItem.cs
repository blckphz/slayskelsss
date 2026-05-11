using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Useable Item")]
public class UseableItem : ItemData
{
    [Header("Ability to Trigger")]
    public Ability abilityToExecute;

    // This property maps the Ability's fireRate to a name the Hotbar can use
    public float useRate => abilityToExecute != null ? abilityToExecute.fireRate : 0.1f;

    public override void Use(Transform caster, Transform targetAnchor)
    {
        if (InventoryManager.Instance == null) return;

        if (abilityToExecute == null)
        {
            Debug.LogWarning($"[Item System] {itemName} has no Ability assigned!");
            return;
        }

        if (GetCurrentCount() <= 0) return;

        bool hasExecuted = abilityToExecute.Execute(caster, targetAnchor, true);

        if (!hasExecuted)
        {
            Debug.Log("[Item System] Ability failed to execute.");
        }
    }

    private int GetCurrentCount()
    {
        if (InventoryManager.Instance == null) return 0;
        int totalFound = 0;

        foreach (var slot in InventoryManager.Instance.inventory)
        {
            if (slot.item != null && slot.item.itemID == this.itemID)
                totalFound += slot.count;
        }

        foreach (var hSlot in InventoryManager.Instance.hotbarData)
        {
            if (hSlot.item != null && hSlot.item.itemID == this.itemID)
                totalFound += hSlot.count;
        }

        return totalFound;
    }
}