using UnityEngine;

public class LootBonusManager : MonoBehaviour
{
    public static LootBonusManager Instance;

    [Header("Global Loot Bonus")]
    public int extraLoot = 0;

    private void Awake()
    {
        Instance = this;
        Debug.Log("[LootBonusManager] Initialized.");
    }

    public int GetBonusLoot()
    {
        return extraLoot;
    }

    public void AddLootBonus(int amount)
    {
        extraLoot += amount;
        Debug.Log($"[LootBonusManager] +{amount} loot bonus applied. Total bonus: {extraLoot}");
    }

    public void RemoveLootBonus(int amount)
    {
        extraLoot -= amount;
        Debug.Log($"[LootBonusManager] -{amount} loot bonus removed. Total bonus: {extraLoot}");
    }
}