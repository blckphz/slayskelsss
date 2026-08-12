using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> tools = new List<ItemData>();
    [SerializeField] private List<ItemData> placeables = new List<ItemData>();
    [SerializeField] private List<ItemData> resources = new List<ItemData>();

    // hidden master list used for lookup
    public List<ItemData> allItems = new List<ItemData>();

    public IReadOnlyList<ItemData> Tools => tools;
    public IReadOnlyList<ItemData> Placeables => placeables;
    public IReadOnlyList<ItemData> Resources => resources;

    private void OnEnable()
    {
        RebuildDatabase();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildDatabase();
    }
#endif

    private void RebuildDatabase()
    {
        allItems.Clear();

        AddRangeNoDuplicates(tools);
        AddRangeNoDuplicates(placeables);
        AddRangeNoDuplicates(resources);
    }

    private void AddRangeNoDuplicates(List<ItemData> list)
    {
        foreach (var item in list)
        {
            if (item == null) continue;

            if (!allItems.Contains(item))
                allItems.Add(item);
        }
    }

    public ItemData GetItemByID(int id)
    {
        return allItems.Find(item => item.itemID == id);
    }
}