using UnityEngine;

[CreateAssetMenu(fileName = "New Berry", menuName = "Abilities/Berry")]
public class berrySO : offensiveRanged
{
    [Header("Eat Settings")]
    public float healAmount = 20f;

    // PRIMARY ACTION: The Throw
    // This uses 'damage' and 'projectileSpeed' which are inherited from offensiveRanged
    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (prefab == null) return false;

        Vector2 direction = (targetAnchor.position - caster.position).normalized;
        GameObject berry = ObjectPooler.Instance.GetPooledObject(prefab, caster.position, Quaternion.identity);

        if (berry != null && berry.TryGetComponent<berryBehaviour>(out var behavior))
        {
            // Setup uses the variables inherited from offensiveRanged/offensiveability
            behavior.Setup(damage, projectileSpeed, direction);
        }
        return true;
    }

    // SECONDARY ACTION: The Eat
    // This is the "Secondary Effect" logic
    public override bool ExecuteSecondary(Transform caster)
    {
        // Try to get playerHealth specifically first to trigger UI
        if (caster.TryGetComponent<playerHealth>(out playerHealth pHealth))
        {
            if (pHealth.currentHealth >= pHealth.maxHealth) return false;

            pHealth.Heal(healAmount); // This calls the UI update!
            return true;
        }
        // Fallback for enemies or other things with healthMaster
        else if (caster.TryGetComponent<healthMaster>(out healthMaster genericHealth))
        {
            if (genericHealth.currentHealth >= genericHealth.maxHealth) return false;
            genericHealth.currentHealth = Mathf.Min(genericHealth.currentHealth + healAmount, genericHealth.maxHealth);
            return true;
        }
        return false;
    }
}