using System.Collections.Generic;

[System.Serializable]
public class ChestSlot
{
    public ItemData item;
    public int count;
    public ChestSlot(ItemData i, int c) { item = i; count = c; }
}

