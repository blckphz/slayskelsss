using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(
    menuName = "Inventory/Build Item/BasicBuildItem"
)]
public class buildSO : ItemData
{
    [Header("Building")]
    public GameObject placeablePrefab;

    [Header("Grid Size")]
    public Vector2Int size = Vector2Int.one;

    public AudioClip placementSound;

    [Header("Placement")]
    public bool supportsFreePlacement = true;
    public bool baseTile = false;
    public string baseTileName = "floorTiles";
}