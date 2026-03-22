using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Resource Item")]
public class ResourceSO : ItemData
{
    [Header("Resource Details")]

    public bool isBurnable; // Maybe for a campfire later?
}