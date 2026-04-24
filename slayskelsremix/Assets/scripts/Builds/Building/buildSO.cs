using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Build Item")]
public class buildSO : ItemData
{
    [Header("Building")]
    public GameObject placeablePrefab;

    [Header("Grid Size")]
    public Vector2Int size = Vector2Int.one;

    public AudioClip placementSound;
}