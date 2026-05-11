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

    // 🔥 MUST return success/failure
    public virtual bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        return true;
    }
}