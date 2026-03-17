using UnityEngine;

public class WoodPickup : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private ItemData woodData;
    [SerializeField] private int amount = 1;

    [Header("Pickup Mode")]
    [SerializeField] private bool autoPickupOnCollision = true;

    public string InteractionPrompt => $"[E] Pick up {amount} {woodData.itemName}";

    // --- OPTION 1: Manual Interact (Keep for compatibility) ---
    public void Interact(InventoryManager playerInventory)
    {
        Pickup(playerInventory);
    }

    // --- OPTION 2: Auto Pickup on Collision ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (autoPickupOnCollision)
        {
            // Check if the object we hit has an InventoryManager
            if (collision.TryGetComponent(out InventoryManager playerInventory))
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
            Debug.Log($"<color=green>[Success]</color> Added {amount} {woodData.itemName} to inventory.");
            playerInventory.AddItem(woodData, amount);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError($"No ItemData assigned to {gameObject.name}!");
        }
    }
}