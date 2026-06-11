using UnityEngine;

[System.Serializable]
public class ItemInstance : IItemInstance
{
    [SerializeField] private ItemData blueprint;
    [SerializeField] private float currentDurability;
    [SerializeField] private int stackCount;

    // WATER STATE (NOW HERE - FIXED)
    [SerializeField] private float currentWater;

    public ItemData Blueprint => blueprint;

    public float CurrentDurability
    {
        get => currentDurability;
        set => currentDurability = Mathf.Clamp(value, 0f, MaxDurability);
    }

    public float MaxDurability =>
        blueprint != null ? blueprint.maxDurability : 0f;

    public int StackCount
    {
        get => stackCount;
        set => stackCount = Mathf.Max(0, value);
    }

    public float CurrentWater
    {
        get => currentWater;
        set => currentWater = Mathf.Clamp(value, 0f, MaxWater);
    }

    public float MaxWater =>
        blueprint is buckeSO bucket ? bucket.maxWater : 0f;

    public bool IsBroken =>
        MaxDurability > 0f && CurrentDurability <= 0.01f;

    // CONSTRUCTOR
    public ItemInstance(ItemData itemAsset, int count = 1, float durability = -1f)
    {
        blueprint = itemAsset;
        stackCount = Mathf.Max(1, count);

        if (itemAsset != null)
        {
            currentDurability = durability >= 0f
                ? durability
                : itemAsset.maxDurability;

            currentWater = 0f;

            Debug.Log($"[ItemInstance] Created: {itemAsset.name}, Durability={currentDurability}");
        }
    }

    // DURABILITY
    public void UseDurability(float amount)
    {
        if (MaxDurability <= 0f)
        {
            Debug.Log("[ItemInstance] No durability system on this item.");
            return;
        }

        CurrentDurability -= amount;

        Debug.Log($"[ItemInstance] {Blueprint?.name} used durability -{amount}. Now: {CurrentDurability}");
    }

    // WATER FILL (IMPORTANT FIX)
    public void FillBucket(float amount)
    {
        if (MaxWater <= 0f)
        {
            Debug.LogWarning($"[ItemInstance] {Blueprint?.name} is not a bucket.");
            return;
        }

        float before = currentWater;
        CurrentWater += amount;

        Debug.Log($"[ItemInstance] Bucket filled: {before} → {CurrentWater}");
    }
}