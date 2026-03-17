using UnityEngine;

[CreateAssetMenu(fileName = "NewDefensiveAbility", menuName = "Abilities/Defensive")]
public class defensiveAbilities : Ability
{
    public float hp;

    // Added 'bool isHolding' to match the updated Ability base class
    public override void Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // For most defensive abilities, you only want it to fire once per press
        // You can check 'if(isHolding)' or just leave it as is if 
        // the PlayerAttack cooldown handles the timing.
        Debug.Log("Executing Defensive Ability");
    }
}