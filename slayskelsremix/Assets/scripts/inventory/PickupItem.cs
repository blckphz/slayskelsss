using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemData woodData;
    [SerializeField] private int amount = 1;

    [Header("Pickup Mode")]
    [SerializeField] private bool autoPickupOnCollision = true;

    private Highlightable highlight;

    // NEW: prevents double pickup
    private bool isPickedUp = false;

    private void Awake()
    {
        highlight = GetComponent<Highlightable>();
    }

    public ItemData GetItemData() => woodData;

    public string GetPrompt()
    {
        return woodData != null
            ? $"[E] Pick up {amount} {woodData.itemName}"
            : "Empty Item";
    }

    public void Interact(InventoryManager playerInventory)
    {
        if (woodData == null || isPickedUp) return;

        Pickup(playerInventory);
    }

    public void OnFocus() => highlight?.SetHighlighted(true);
    public void OnLoseFocus() => highlight?.SetHighlighted(false);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!autoPickupOnCollision || isPickedUp) return;

        // Player pickup
        if (collision.TryGetComponent(out InventoryManager playerInv))
        {
            Pickup(playerInv);
            return;
        }

        // NPC pickup
        if (collision.TryGetComponent(out NpcInvBrain npcInv))
        {
            npcInv.AddItem(woodData, amount);

            if (collision.TryGetComponent(out NPCBrain brain))
            {
                brain.WakeUp();
            }

            isPickedUp = true;
            Destroy(gameObject);
        }
    }

    private void Pickup(InventoryManager playerInventory)
    {
        if (woodData == null || isPickedUp) return;

        isPickedUp = true;

        playerInventory.AddItem(woodData, amount);
        Destroy(gameObject);
    }
}