using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Bucket Item")]
public class bucketSO : UseableItem
{
    [Header("Bucket Settings")]
    public float maxWater = 100f;

    public override bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        Debug.Log($"[BucketItem] Use triggered on {itemName}");

        var instance = target?.GetComponent<ItemInstance>();
        if (instance == null)
        {
            Debug.LogWarning("[BucketItem] No ItemInstance found on target!");
            return false;
        }

        if (instance is BucketItemInstance bucket)
        {
            bucket.Water = Mathf.Max(0, bucket.Water - 10f);
            Debug.Log($"[BucketItem] Water reduced → {bucket.Water}");
            return true;
        }

        Debug.LogWarning("[BucketItem] ItemInstance is not BucketInstance!");
        return false;
    }
}