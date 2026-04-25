using UnityEngine;


[CreateAssetMenu(fileName = "NewTurretPerk", menuName = "Perks/Turret/Health")]
public class TurretFortressUpgrade : AbilityUpgradeSO
{
    public float hpBonus = 50f;
    public int countBonus = 2;

    public override void Apply(Ability ability)
    {
        if (ability is turretSO turret)
        {
            turret.hp += hpBonus;
            turret.maxturretcount += countBonus;
        }
    }
}