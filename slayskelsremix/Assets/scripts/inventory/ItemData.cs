using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public Sprite icon;

    [Header("Type")]
    public ItemType itemType;

    [Header("Building")]
    public GameObject placeablePrefab;

    [Header("Consumable")]
    public int healAmount;
}