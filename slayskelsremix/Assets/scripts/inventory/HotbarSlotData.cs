using UnityEngine;

[System.Serializable]
public class HotbarSlotData
{
    public ItemData item;
    public Ability ability;

    public int count;
    public int currentDurability;

    public bool IsEmpty => item == null && ability == null;

    public void Clear()
    {
        item = null;
        ability = null;
        count = 0;
        currentDurability = 0;
    }
}