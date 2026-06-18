using UnityEngine;

[System.Serializable]
public class ItemInstance : IItemInstance
{
    [SerializeField] private ItemData blueprint;
    [SerializeField] private float currentDurability;
    [SerializeField] private int stackCount;

    // Interface Properties
    public ItemData Blueprint => blueprint;
    public float CurrentDurability
    {
        get => currentDurability;
        set => currentDurability = Mathf.Clamp(value, 0f, MaxDurability);
    }
    public float MaxDurability => blueprint != null ? blueprint.maxDurability : 0f;
    public int StackCount
    {
        get => stackCount;
        set => stackCount = value;
    }

    public bool IsBroken => MaxDurability > 0f && CurrentDurability <= 0f;

    // Constructor to generate a fresh instance from an asset blueprint
    public ItemInstance(ItemData itemAsset, int count = 1)
    {
        blueprint = itemAsset;
        stackCount = count;

        if (itemAsset != null)
        {
            currentDurability = itemAsset.maxDurability; // Set fresh out of the box
        }
    }


    public void UseDurability(float amount)
    {
        if (MaxDurability <= 0f) return; // Non-durable item
        CurrentDurability -= amount;
    }
}