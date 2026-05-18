using UnityEngine;

[CreateAssetMenu(fileName = "New Berry", menuName = "Abilities/Berry")]
public class berrySO : offensiveRanged, IItemDescriptionProvider
{
    [Header("Eat Settings")]
    public float healAmount = 20f;

    [Header("Planting Settings")]
    public GameObject plantPrefab; // The crop prefab to spawn

    // ✅ MODULAR INTERFACE WITH STATS & DESCRIPTION
    public string GetDetailedDescription()
    {
        string lines = "";

        // Show core numerical values
        lines += $"<color=#FF6B6B>Damage:</color> {damage}\n";
        lines += $"<color=#A2E8DD>Fire Rate:</color> {fireRate}s\n";
        lines += "------------------\n"; // Visual barrier

        // Custom action keys
        lines += "<color=#FFA500><b>[PRIMARY ACTIONS]</b></color>\n";
        lines += "• <color=#A2E8DD>Plant:</color> Place in dug holes to grow crops.\n";
        lines += $"• <color=#FF6B6B>Throw:</color> Toss at enemies for ranged impact.\n\n";

        lines += "<color=#FFA500><b>[SECONDARY ACTION]</b></color>\n";
        lines += $"• <color=#4CAF50>Eat:</color> Restores <color=#98FB98>+{healAmount} HP</color>.\n\n";

        return lines;
    }

    // PRIMARY ACTION (Left Click or Interact)
    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        GameObject currentTarget = PlayerHotbarManager.Instance.interaction.CurrentTarget;

        if (currentTarget != null && currentTarget.TryGetComponent(out EarthHoleDigBehav hole))
        {
            return hole.PlantSeed(plantPrefab);
        }

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