using UnityEngine;

[CreateAssetMenu(fileName = "New Berry", menuName = "Abilities/Berry")]
public class berrySO : offensiveRanged, IItemDescriptionProvider
{
    [Header("Eat Settings")]
    public int healAmount = 5;

    [Header("Planting Settings")]
    public GameObject plantPrefab;

    // =========================================================
    // DESCRIPTION
    // =========================================================
    public string GetDetailedDescription()
    {
        string lines = "";

        lines += $"<color=#FF6B6B>Damage:</color> {damage}\n";
        lines += $"<color=#A2E8DD>Fire Rate:</color> {fireRate}s\n";
        lines += "------------------\n";

        lines += "<color=#FFA500><b>[PRIMARY ACTIONS]</b></color>\n";
        lines += "• Plant: Place in dug holes to grow crops.\n";
        lines += "• Throw: Ranged attack projectile.\n\n";

        lines += "<color=#FFA500><b>[SECONDARY ACTION]</b></color>\n";
        lines += $"• Eat: Restores +{healAmount} HP.\n";

        return lines;
    }

    // =========================================================
    // PRIMARY ACTION
    // =========================================================
    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // 🔥 SAFE TARGET ACCESS (NO HOTBAR DEPENDENCY)
        GameObject currentTarget =
            Object.FindFirstObjectByType<PlayerInteraction2D>()?.CurrentTarget;

        Debug.Log($"[BerrySO] Execute called. Target: {(currentTarget ? currentTarget.name : "NULL")}");

        // -----------------------------------------------------
        // PLANTING
        // -----------------------------------------------------
        if (currentTarget != null &&
            currentTarget.TryGetComponent(out EarthHoleDigBehav hole))
        {
            Debug.Log("[BerrySO] Planting seed in hole.");
            return hole.PlantSeed(plantPrefab);
        }

        // -----------------------------------------------------
        // THROW PROJECTILE
        // -----------------------------------------------------
        if (prefab == null)
        {
            Debug.LogWarning("[BerrySO] No projectile prefab assigned!");
            return false;
        }

        Vector2 direction =
            (targetAnchor.position - caster.position).normalized;

        GameObject berry = ObjectPooler.Instance.GetPooledObject(
            prefab,
            caster.position,
            Quaternion.identity
        );

        if (berry != null && berry.TryGetComponent<berryBehaviour>(out var behavior))
        {
            behavior.Setup(damage, projectileSpeed, direction);
            Debug.Log("[BerrySO] Projectile thrown.");
        }
        else
        {
            Debug.LogWarning("[BerrySO] Failed to spawn projectile or missing berryBehaviour.");
        }

        return true;
    }

    // =========================================================
    // SECONDARY ACTION
    // =========================================================
    public override bool ExecuteSecondary(Transform caster)
    {
        if (caster == null)
        {
            Debug.LogWarning("[BerrySO] No caster provided.");
            return false;
        }

        if (caster.TryGetComponent<playerHealth>(out playerHealth pHealth))
        {
            if (pHealth.currentHealth >= pHealth.maxHealth)
            {
                Debug.Log("[BerrySO] Already full health.");
                return false;
            }

            pHealth.Heal(healAmount);

            Debug.Log($"[BerrySO] Healed for {healAmount}");
            return true;
        }

        Debug.LogWarning("[BerrySO] No playerHealth found on caster.");
        return false;
    }
}