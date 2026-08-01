using UnityEngine;

[CreateAssetMenu(fileName = "New Berry", menuName = "Abilities/Berry")]
public class berrySO : offensiveRanged, IItemDescriptionProvider
{
    [Header("Eat Settings")]
    public int healAmount = 5;
    public int hungerAmount = 20;

    [Header("Inventory")]
    public ItemData berryItem;

    [Header("Planting Settings")]
    public GameObject plantPrefab;


    public string GetDetailedDescription()
    {
        return $"<color=#FF6B6B>Damage:</color> {damage}\n" +
               $"<color=#A2E8DD>Fire Rate:</color> {fireRate}s\n" +
               "------------------\n" +
               "<color=#FFA500><b>[PRIMARY ACTIONS]</b></color>\n" +
               "• Plant: Place in dug holes to grow crops.\n" +
               "• Throw: Ranged attack projectile.\n\n" +
               "<color=#FFA500><b>[SECONDARY ACTION]</b></color>\n" +
               $"• Eat: Restores +{healAmount} HP and +{hungerAmount} Hunger.";
    }



    public override bool Execute(
        Transform caster,
        Transform targetAnchor,
        bool isHolding)
    {
        GameObject currentTarget =
            Object.FindFirstObjectByType<PlayerInteraction2D>()
            ?.CurrentTarget;


        if (currentTarget != null &&
            currentTarget.TryGetComponent(out EarthHoleDigBehav hole))
        {
            return hole.PlantSeed(plantPrefab);
        }


        if (prefab == null)
            return false;


        Vector2 direction =
            (targetAnchor.position - caster.position).normalized;


        GameObject berry =
            ObjectPooler.Instance.GetPooledObject(
                prefab,
                caster.position,
                Quaternion.identity
            );


        if (berry != null &&
            berry.TryGetComponent<berryBehaviour>(out var behavior))
        {
            behavior.Setup(
                damage,
                projectileSpeed,
                direction
            );
        }


        return true;
    }



    public override bool ExecuteSecondary(Transform caster)
    {
        playerHealth pHealth = caster.GetComponent<playerHealth>();
        PlayerNeeds needs = caster.GetComponent<PlayerNeeds>();

        bool ate = false;


        // Heal health
        if (pHealth != null && pHealth.currentHealth < pHealth.maxHealth)
        {
            pHealth.Heal(healAmount);
            ate = true;
        }


        // Restore hunger
        if (needs != null && needs.saturation < needs.maxSaturation)
        {
            needs.AddFood(hungerAmount);
            ate = true;
        }


        // Consume from hotbar
        if (ate && PlayerHotbarManager.Instance != null)
        {
            PlayerHotbarManager.Instance.UseSelectedStack(1);
        }


        return ate;
    }
}