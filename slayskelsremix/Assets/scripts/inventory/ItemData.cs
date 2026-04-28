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
    public bool isUsable;

    [Tooltip("How many items are consumed per use")]
    public int consumeAmount = 1;

    public virtual void Use(Transform caster, Transform targetAnchor)
    {
        Debug.Log($"Used item: {itemName}");
    }
}