using UnityEngine;

public abstract class offensiveability : Ability
{
    [Header("Base Offensive Stats")]
    public float damage;

    // This must match the new 3-argument signature
    public override void Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // This is usually left empty or handles base logic
    }
}