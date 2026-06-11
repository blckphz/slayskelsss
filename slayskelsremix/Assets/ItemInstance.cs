using UnityEngine;

[System.Serializable]
public class ItemInstance : IItemInstance
{
    [SerializeField] private ItemData blueprint;
    [SerializeField] private float currentDurability;
    [SerializeField] private int stackCount;

    // generic per-item runtime value (water, fuel, charges, etc.)
    [SerializeField] private float customValue;

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

    public float CustomValue
    {
        get => customValue;
        set => customValue = value;
    }

    public bool IsBroken =>
        MaxDurability > 0f && CurrentDurability <= 0.01f;

    // MAIN constructor (safe for gameplay + loading)
    public ItemInstance(ItemData itemAsset, int count = 1, float durability = -1f)
    {
        blueprint = itemAsset;
        stackCount = Mathf.Max(1, count);

        if (itemAsset != null)
        {
            currentDurability = durability >= 0f
                ? durability
                : itemAsset.maxDurability;
        }
    }

    public void UseDurability(float amount)
    {
        if (MaxDurability <= 0f) return;

        CurrentDurability -= amount;

        Debug.Log($"[ItemInstance] {Blueprint?.itemName} durability → {CurrentDurability}");
    }
}