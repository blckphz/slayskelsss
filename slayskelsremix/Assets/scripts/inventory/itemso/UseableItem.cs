using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Useable Item")]
public class UseableItem : ItemData
{
    [Header("Ability to Trigger")]
    public Ability abilityToExecute;

    public float useRate => abilityToExecute != null ? abilityToExecute.fireRate : 0.1f;

    private void OnEnable()
    {
        itemType = ItemType.Consumable;
    }

    public override bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        if (abilityToExecute == null)
        {
            Debug.LogWarning($"[UseableItem] {itemName} has no ability!");
            return false;
        }

        abilityToExecute.Execute(caster, targetAnchor, true);
        return true;
    }
}