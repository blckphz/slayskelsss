using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Build Item")]
public class buildSO : ItemData
{
    [Header("Building")]
    public GameObject placeablePrefab;
}