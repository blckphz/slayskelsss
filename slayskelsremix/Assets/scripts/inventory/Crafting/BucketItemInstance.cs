using UnityEngine;

public class BucketItemInstance : ItemInstance
{
    public float Water
    {
        get => CustomValue;
        set => CustomValue = Mathf.Clamp(value, 0f, 100f);
    }

    public BucketItemInstance(ItemData data, int count = 1)
        : base(data, count)
    {
    }
}