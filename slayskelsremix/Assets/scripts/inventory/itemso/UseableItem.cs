using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Useable Item")]
public class UseableItem : ItemData
{
    [Header("Ability to Trigger")]
    public Ability abilityToExecute;

    public float useRate => abilityToExecute != null ? abilityToExecute.fireRate : 0.1f;

    // ✅ FULL CONTEXT-AWARE VERSION
    public override bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        if (abilityToExecute == null)
        {
            Debug.LogWarning($"[Item System] {itemName} has no Ability assigned!");
            return false;
        }

        return true;
    }
}