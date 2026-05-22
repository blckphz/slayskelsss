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

    [Tooltip("How many items are removed per use. Only applies if item is consumable.")]
    public int consumeAmount = 1;

    [Header("Durability")]
    public int maxDurability;
    public bool IsUnbreakable => maxDurability <= 0;

    public virtual bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        return true;
    }
}