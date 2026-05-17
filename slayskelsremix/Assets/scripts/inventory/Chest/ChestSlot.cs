using UnityEngine;

[System.Serializable]
public class ChestSlot
{
    public ItemData item;
    public int count;
    public float durability;

    public ChestSlot(ItemData item, int count, float durability = -1f)
    {
        this.item = item;
        this.count = count;
        this.durability = (durability < 0 && item != null) ? item.maxDurability : durability;
    }
}