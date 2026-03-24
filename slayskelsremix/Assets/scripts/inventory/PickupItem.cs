using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private ItemData woodData;
    [SerializeField] private int amount = 1;

    [Header("Pickup Mode")]
    [SerializeField] private bool autoPickupOnCollision = true;

    public string InteractionPrompt => woodData != null ? $"[E] Pick up {amount} {woodData.itemName}" : "Empty Item";

    // --- Manual Interaction (Player) ---
    public void Interact(InventoryManager playerInventory)
    {
        if (woodData == null) return;

        playerInventory.AddItem(woodData, amount);
        Destroy(gameObject);
    }

    // --- Auto Pickup (Player or NPC) ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!autoPickupOnCollision) return;

        // 1. Check if it's the Player
        InventoryManager playerInv = collision.GetComponent<InventoryManager>();
        if (playerInv != null)
        {
            playerInv.AddItem(woodData, amount);
            Destroy(gameObject);
            return;
        }

        // 2. Check if it's an NPC
        NpcInvBrain npcInv = collision.GetComponent<NpcInvBrain>();
        if (npcInv != null)
        {
            npcInv.AddItem(woodData, amount);
            Destroy(gameObject);
        }
    }
}