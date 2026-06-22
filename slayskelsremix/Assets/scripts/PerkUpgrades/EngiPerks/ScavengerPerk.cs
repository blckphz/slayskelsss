using UnityEngine;

[CreateAssetMenu(fileName = "ScavengerPerk", menuName = "Perks/Passive/Scavenger")]
public class ScavengerPerk : PassivePerkSO
{
    [Header("Scavenger Bonus")]
    public int bonusLoot = 1;

    public override void ApplyPassive()
    {
        if (LootBonusManager.Instance == null)
        {
            Debug.LogWarning("[ScavengerPerk] LootBonusManager missing!");
            return;
        }

        LootBonusManager.Instance.AddLootBonus(bonusLoot);
        Debug.Log("[ScavengerPerk] Applied: +loot bonus");
    }

    public override void RemovePassive()
    {
        if (LootBonusManager.Instance == null)
        {
            Debug.LogWarning("[ScavengerPerk] LootBonusManager missing!");
            return;
        }

        LootBonusManager.Instance.RemoveLootBonus(bonusLoot);
        Debug.Log("[ScavengerPerk] Removed: -loot bonus");
    }
}