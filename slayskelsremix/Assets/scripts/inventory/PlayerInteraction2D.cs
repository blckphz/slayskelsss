using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction2D : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRadius = 1.5f;
    public LayerMask interactableLayer;
    public InventoryManager inventory;

    [Header("Quick Add (Press 1)")]
    [Tooltip("Drag your Wood ItemData ScriptableObject here")]
    public ItemData woodItemData;

    // --- NEW METHOD: This is called when you press '1' ---
    // (Ensure your Input Action is named "AddWood")
    public void OnAddWood(InputValue value)
    {
        if (!value.isPressed) return;

        if (inventory != null && woodItemData != null)
        {
            Debug.Log("<color=cyan>[Hotbar]</color> Pressed 1: Adding 1 Wood manually.");
            inventory.AddItem(woodItemData, 1);
        }
        else
        {
            Debug.LogWarning("<color=red>[Error]</color> Cannot add wood. Check if WoodItemData and Inventory are assigned in the Inspector!");
        }
    }

    // This is called by the PlayerInput component (SendMessage) for [E]
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        Debug.Log("<color=white>[Step 1]</color> Interact Key Pressed (Input System works!)");

        // Check what's in range
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRadius, interactableLayer);

        if (hit != null)
        {
            Debug.Log($"<color=cyan>[Step 2]</color> Found object: {hit.name} on Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

            if (hit.TryGetComponent(out IInteractable interactable))
            {
                Debug.Log("<color=green>[Step 3]</color> IInteractable found! Calling Interact()...");
                interactable.Interact(inventory);
            }
            else
            {
                Debug.LogWarning("<color=orange>[Error]</color> Hit object has no IInteractable script attached!");
            }
        }
        else
        {
            Debug.Log("<color=red>[Error]</color> Nothing in range. Check: 1. Radius, 2. Layer assigned to Wood, 3. LayerMask in Inspector.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}