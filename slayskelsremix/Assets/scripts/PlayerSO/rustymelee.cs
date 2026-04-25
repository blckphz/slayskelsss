using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAbility", menuName = "Abilities/Melee")]
public class rustymelee : offensivemelee
{
    [Header("Rusty Melee Specials")]
    public float bonusdmg;

    public override float GetBonusDamage()
    {
        return bonusdmg;
    }
}