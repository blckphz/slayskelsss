using UnityEngine;

public interface IAbilityUpgrade
{
    string UpgradeName { get; }
    string Description { get; }
    void Apply(Ability ability);
}

// A generic upgrade for any offensive ability
public class DamageUpgrade : IAbilityUpgrade
{
    public string UpgradeName => "Increased Power";
    public string Description => "Increases damage by 10 points.";
    private float _amount = 10f;

    public void Apply(Ability ability)
    {
        if (ability is offensiveability off)
        {
            off.damage += _amount;
            Debug.Log($"{ability.name} damage is now {off.damage}");
        }
    }
}

// A specific upgrade for Chain Lightning
public class BounceUpgrade : IAbilityUpgrade
{
    public string UpgradeName => "High Voltage";
    public string Description => "Chain lightning bounces 2 more times.";
    private int _bounces = 2;

    public void Apply(Ability ability)
    {
        if (ability is ChainLightning lightning)
        {
            lightning.maxBounces += _bounces;
            Debug.Log($"{ability.name} max bounces is now {lightning.maxBounces}");
        }
    }
}