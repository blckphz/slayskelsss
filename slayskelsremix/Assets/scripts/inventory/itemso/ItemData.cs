using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public int itemID;
    public string itemName;
    [TextArea] public string ItemDescription;
    public Sprite icon;

    [Header("Audio")]
    public AudioClip DragStartSound;
    public AudioClip DragEndSound;

    [Header("Type")]
    public ItemType itemType;

    [Header("Stacking")]
    public int maxStackSize = 99;

    [Tooltip("If false, item ignores durability completely (always stacks normally).")]
    public bool usesDurability = false;

    [Header("Durability")]
    public int maxDurability;

    public bool IsUnbreakable => !usesDurability || maxDurability <= 0;

    [Header("Usage")]
    public bool isUsable = true;

    [Tooltip("How many items are removed per use.")]
    public int consumeAmount = 1;

    public virtual bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        return true;
    }
}