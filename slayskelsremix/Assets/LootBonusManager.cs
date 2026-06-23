using UnityEngine;

public class LootBonusManager : MonoBehaviour
{
    public static LootBonusManager Instance;

    [Header("Global Loot Bonus")]
    public int extraLoot;

    private void Awake()
    {
        Instance = this;
        Debug.Log("[LootBonusManager] Initialized.");
    }

    public void SetLootBonus(int value)
    {
        extraLoot = value;
        Debug.Log($"[LootBonusManager] Loot bonus set to: {extraLoot}");
    }

    public int GetBonusLoot()
    {
        return extraLoot;
    }
}