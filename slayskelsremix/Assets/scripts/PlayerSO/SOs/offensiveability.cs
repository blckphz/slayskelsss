using UnityEngine;

public abstract class offensiveability : Ability
{
    [Header("Base Offensive Stats")]
    public float damage;

    // Now returns bool to match the base class
    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        return false;
    }
}