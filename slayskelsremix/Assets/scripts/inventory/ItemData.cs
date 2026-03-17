using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int itemID; // Give each item a unique number (Wood = 1, Stone = 2, etc.)
    public string itemName;
    public Sprite icon;
}