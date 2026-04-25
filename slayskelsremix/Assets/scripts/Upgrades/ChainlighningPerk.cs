using UnityEngine;

[CreateAssetMenu(fileName = "ChainLightningBounceUpgrade", menuName = "Perks/ChainLightning/MoreBounces")]
public class ChainlightningPerk : AbilityUpgradeSO
{
    [Header("Upgrade Stats")]
    public int bonusBounces = 2;
    public float radiusIncrease = 1.5f;

    public override void Apply(Ability ability)
    {
        // Use 'as' to check if the ability is actually Chain Lightning
        if (ability is ChainLightning lightning)
        {
            lightning.maxBounces += bonusBounces;
            lightning.bounceRadius += radiusIncrease;

            Debug.Log($"<color=cyan>[Perk]</color> Chain Lightning upgraded! " +
                      $"Max Bounces: {lightning.maxBounces}, Radius: {lightning.bounceRadius}");
        }
        else
        {
            Debug.LogWarning("<color=red>[Perk]</color> Target ability is not ChainLightning!");
        }
    }
}