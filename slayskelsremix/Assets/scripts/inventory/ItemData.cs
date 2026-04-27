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
    [Tooltip("How often this item can be used (cooldown in seconds)")]
    public float useRate = 0.2f;

    [Tooltip("How many items are consumed per use")]
    public int consumeAmount = 1;

    // 🔥 MODULAR: Every item can now be "Used"
    // IMPORTANT: Do NOT modify stack count in here
    public virtual void Use(Transform caster, Transform targetAnchor)
    {
        // Default behavior (override in child classes)
        Debug.Log($"Used item: {itemName}");
    }
}