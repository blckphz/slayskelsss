using UnityEngine;

[System.Serializable]
public class HotbarSlotData
{
    public ItemData item;
    public Ability ability;
    public int count; // Holds the stack size (e.g., 3)
    public float currentDurability; // 👈 Upgraded to float to match dynamic runtime tracking loops

    public bool IsEmpty => item == null && ability == null;

    public void Clear()
    {
        item = null;
        ability = null;
        count = 0;
        currentDurability = 0f; // 👈 Clears durability safely using floating-point zero
    }
}