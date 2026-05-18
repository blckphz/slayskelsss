using UnityEngine;

public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public Sprite icon;
    public AudioClip DragStartSound;
    public AudioClip DragEndSound;

    [Header("Type")]
    public ItemType itemType;
    public int maxStackSize = 99;

    [Header("Usage")]
    public bool isUsable = true;
    public int consumeAmount = 1;

    [Tooltip("Set to 0 if the item is unbreakable (e.g., resources, quest items, or basic consumables)")]
    public int maxDurability;

    // Helper property to make your code much easier to read down the line
    public bool IsUnbreakable => maxDurability <= 0;

    // Returns true if the action successfully executed
    public virtual bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        return true;
    }
}