using UnityEngine;

[System.Serializable]
public class ChestSlot
{
    public ItemData item;
    public int count;
    public int durability;

    public ChestSlot(ItemData item, int count, int durability = -1)
    {
        this.item = item;
        this.count = count;
        this.durability = (durability < 0 && item != null) ? item.maxDurability : durability;
    }
}