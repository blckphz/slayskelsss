using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private ItemData woodData;
    [SerializeField] private int amount = 1;

    [Header("Pickup Mode")]
    [SerializeField] private bool autoPickupOnCollision = true;

    // The prompt shown to the player if they use manual interaction
    public string InteractionPrompt => woodData != null ? $"[E] Pick up {amount} {woodData.itemName}" : "Empty Item";

    // --- OPTION 1: Manual Interaction ---
    public void Interact(InventoryManager playerInventory)
    {
        Pickup(playerInventory);
    }

    // --- OPTION 2: Auto Pickup on Collision ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (autoPickupOnCollision)
        {
            // We check for the InventoryManager on the object that hit us (the Player)
            InventoryManager playerInventory = collision.GetComponent<InventoryManager>();

            if (playerInventory != null)
            {
                Debug.Log($"<color=orange>[Auto-Pickup]</color> Collided with {collision.name}");
                Pickup(playerInventory);
            }
        }
    }

    private void Pickup(InventoryManager playerInventory)
    {
        if (woodData != null)
        {
            // ⚡ HOW STACKING WORKS:
            // This calls AddItem in your InventoryManager. 
            // Because your InventoryManager has a loop that checks: 
            // 'if (slot.item == data) { slot.count += amount; }'
            // the item will automatically stack in the existing slot.

            playerInventory.AddItem(woodData, amount);

            Debug.Log($"<color=green>[Success]</color> Added {amount} {woodData.itemName}. Stack updated via InventoryManager.");

            // Destroy the world object after it has been added to the data
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError($"No ItemData assigned to the PickupItem script on {gameObject.name}!");
        }
    }
}