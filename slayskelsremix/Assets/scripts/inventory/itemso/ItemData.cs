using UnityEngine;

public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public Sprite icon;

    [Header("Type")]
    public ItemType itemType;
    public int maxStackSize = 99;

    [Header("Usage")]
    public bool isUsable = true;
    public int consumeAmount = 1;

    [Tooltip("Set to 0 if the item is unbreakable (e.g., resources or basic consumables)")]
    public int maxDurability;

    [Tooltip("The current runtime durability of this specific tool instance")]
    public float currentDurability; // Added to support your durability logging!

    // Returns true if the action successfully executed
    public virtual bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        return true;
    }
}