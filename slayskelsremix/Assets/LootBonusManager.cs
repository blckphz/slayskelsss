using UnityEngine;

public class LootBonusManager : MonoBehaviour
{
    public static LootBonusManager Instance;

    [Header("Global Loot Bonus")]
    public int extraLoot;

    private void Awake()
    {
        Instance = this;
    }

    public void SetLootBonus(int value)
    {
        extraLoot = value;
    }

    public int GetBonusLoot()
    {
        return extraLoot;
    }
}