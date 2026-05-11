using UnityEngine;

[CreateAssetMenu(fileName = "New Berry", menuName = "Abilities/Berry")]
public class berrySO : offensiveRanged
{
    [Header("Eat Settings")]
    public float healAmount = 20f;

    [Header("Planting Settings")]
    public GameObject plantPrefab; // The crop prefab to spawn

    // PRIMARY ACTION (Left Click or Interact)
    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // 1. Try to see if we are looking at a hole via the interaction system
        GameObject currentTarget = PlayerHotbarManager.Instance.interaction.CurrentTarget;

        if (currentTarget != null && currentTarget.TryGetComponent(out EarthHoleDigBehav hole))
        {
            // If it's a hole, try to plant
            return hole.PlantSeed(plantPrefab);
        }

        // 2. Fallback: Throw the berry if not looking at a hole
        if (prefab == null) return false;

        Vector2 direction = (targetAnchor.position - caster.position).normalized;
        GameObject berry = ObjectPooler.Instance.GetPooledObject(prefab, caster.position, Quaternion.identity);

        if (berry != null && berry.TryGetComponent<berryBehaviour>(out var behavior))
        {
            behavior.Setup(damage, projectileSpeed, direction);
        }
        return true;
    }

    // SECONDARY ACTION (Right Click)
    public override bool ExecuteSecondary(Transform caster)
    {
        if (caster.TryGetComponent<playerHealth>(out playerHealth pHealth))
        {
            if (pHealth.currentHealth >= pHealth.maxHealth) return false;
            pHealth.Heal(healAmount);
            return true;
        }
        return false;
    }
}