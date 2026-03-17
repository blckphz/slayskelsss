using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems = new List<ItemData>();

    public ItemData GetItemByID(int id)
    {
        return allItems.Find(item => item.itemID == id);
    }
}