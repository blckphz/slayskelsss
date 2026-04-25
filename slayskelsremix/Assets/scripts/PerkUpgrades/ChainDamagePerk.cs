using UnityEngine;

[CreateAssetMenu(fileName = "ChainDamagePerk", menuName = "Perks/Chain Damage")]
public class ChainDamagePerk : AbilityUpgradeSO
{
    [Header("Scaling")]
    public float bonusPerLevel = 2f;

    public override void Apply(Ability targetAbility)
    {
        Debug.Log("[ChainDamagePerk] Apply() CALLED");

        if (!chainController.isUnlocked)
        {
            chainController.isUnlocked = true;
            chainController.hitCounter = 0;
            chainController.staticBonusDmg = 0f;

            Debug.Log("[ChainDamagePerk] System UNLOCKED");
        }

        float before = chainController.staticBonusDmg;
        chainController.staticBonusDmg += bonusPerLevel;

        Debug.Log($"[ChainDamagePerk] Bonus DMG: {before} → {chainController.staticBonusDmg}");

        // 🔥 FORCE UI REFRESH
        if (charsetter.Instance != null)
        {
            charsetter.Instance.ForceChargeUIRefresh();
            Debug.Log("[ChainDamagePerk] UI refresh triggered");
        }
    }
}