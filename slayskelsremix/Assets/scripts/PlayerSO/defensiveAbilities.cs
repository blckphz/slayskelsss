using UnityEngine;

[CreateAssetMenu(fileName = "NewDefensiveAbility", menuName = "Abilities/Defensive")]
public class defensiveAbilities : Ability
{
    public float hp;

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // Example: Spawn a shield or heal the player
        Debug.Log("Executing Defensive Ability: " + this.name);

        // Add your specific defensive logic here (instantiating shields, etc.)

        // Return true to trigger the cooldown immediately
        return true;
    }
}