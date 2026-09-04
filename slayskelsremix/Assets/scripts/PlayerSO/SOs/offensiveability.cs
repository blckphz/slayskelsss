using UnityEngine;

public abstract class offensiveability : Ability
{
    [Header("Base Offensive Stats")]
    public int damage;

    public override bool Execute(
        Transform caster,
        Transform targetAnchor,
        bool isHolding)
    {
        return false;
    }
}