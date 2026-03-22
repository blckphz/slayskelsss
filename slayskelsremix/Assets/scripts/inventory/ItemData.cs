using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public Sprite icon;

    [Header("Type")]
    public ItemType itemType;
    public int maxStackSize = 99;

    [Header("Consumable Settings")]
    public int healAmount;

    // 🔥 MODULAR: Every item can now be "Used"
    // We pass caster and anchor so the item knows WHERE to spawn/effect
    public virtual void Use(Transform caster, Transform targetAnchor)
    {
        // Default behavior: Maybe just a log or a generic sound
        Debug.Log($"Using {itemName}. No specific effect assigned.");
    }
}