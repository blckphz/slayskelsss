using UnityEngine;

[CreateAssetMenu(fileName = "ScavengerPerk", menuName = "Perks/Passive/Scavenger")]
public class ScavengerPerk : PassivePerkSO
{
    public int bonusPerLevel = 1;

    public override void ApplyPassive()
    {
        if (LootBonusManager.Instance == null)
        {
            return;
        }

        int totalBonus = Level * bonusPerLevel;

        LootBonusManager.Instance.SetLootBonus(totalBonus);

    }

    public override void RemovePassive()
    {
        if (LootBonusManager.Instance == null)
            return;

        LootBonusManager.Instance.SetLootBonus(0);

        Debug.Log("[ScavengerPerk] Removed passive bonus");
    }
}