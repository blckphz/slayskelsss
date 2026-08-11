using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Build Item/BasicBuildItem")]
public class buildSO : ItemData
{
    [Header("Building")]
    public GameObject placeablePrefab;

    [Header("Grid Size")]
    public Vector2Int size;

    public AudioClip placementSound;

    [Header("Placement")]
    public bool supportsFreePlacement = true;
    public bool baseTile = false;
}