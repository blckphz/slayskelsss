using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAbility", menuName = "Abilities/Melee")]
public class rustymelee : offensivemelee
{
    [Header("Rusty Melee Specials")]
    public float bonusdmg;

    // This "sends" the bonusdmg value up to the PerformSingleSwing method
    public override float GetBonusDamage()
    {
        return bonusdmg;
    }
}