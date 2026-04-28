using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemData woodData;
    [SerializeField] private int amount = 1;

    [Header("Pickup Mode")]
    [SerializeField] private bool autoPickupOnCollision = true;

    private Highlightable highlight;

    private void Awake()
    {
        highlight = GetComponent<Highlightable>();
    }

    // Public getter so npcJobBrain can check if this is Wood
    public ItemData GetItemData() => woodData;

    public string GetPrompt()
    {
        return woodData != null
            ? $"[E] Pick up {amount} {woodData.itemName}"
            : "Empty Item";
    }

    public void Interact(InventoryManager playerInventory)
    {
        if (woodData == null) return;

        playerInventory.AddItem(woodData, amount);
        Destroy(gameObject);
    }

    public void OnFocus() => highlight?.SetHighlighted(true);
    public void OnLoseFocus() => highlight?.SetHighlighted(false);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!autoPickupOnCollision) return;

        // --- Player Pickup ---
        if (collision.TryGetComponent(out InventoryManager playerInv))
        {
            playerInv.AddItem(woodData, amount);
            Destroy(gameObject);
            return;
        }

        // --- NPC Pickup ---
        if (collision.TryGetComponent(out NpcInvBrain npcInv))
        {
            npcInv.AddItem(woodData, amount);

            // Notify the brain to re-evaluate its job now that it has items
            if (collision.TryGetComponent(out NPCBrain brain))
            {
                brain.WakeUp();
            }

            // Clean up global events if you are using them for pathfinding tracking
            // NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID()); 

            Destroy(gameObject);
        }
    }
}