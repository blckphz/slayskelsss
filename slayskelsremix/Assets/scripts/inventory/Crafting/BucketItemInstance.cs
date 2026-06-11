using UnityEngine;

public class BucketItemInstance : ItemInstance
{
    [SerializeField] private float water;

    public float MaxWater =>
        Blueprint is buckeSO bucket ? bucket.maxWater : 0f;

    public float Water
    {
        get => water;
        set
        {
            float before = water;
            water = Mathf.Clamp(value, 0f, MaxWater);

            Debug.Log($"[BucketItemInstance] Water changed: {before} → {water} (Max: {MaxWater})");
        }
    }

    public BucketItemInstance(ItemData data, int count = 1)
        : base(data, count)
    {
        water = 0f;

        Debug.Log($"[BucketItemInstance] Created bucket instance: {data?.name}");
    }

    public void Fill(float amount)
    {
        float before = Water;
        Water += amount;

        Debug.Log($"[BucketItemInstance] Filled bucket: {before} → {Water}");
    }

    public void Drain(float amount)
    {
        float before = Water;
        Water -= amount;

        Debug.Log($"[BucketItemInstance] Drained bucket: {before} → {Water}");
    }
}