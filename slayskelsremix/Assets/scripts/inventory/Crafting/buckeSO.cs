using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Bucket Item")]
public class buckeSO : UseableItem
{
    public float maxWater = 100f;



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