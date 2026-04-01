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

    // --- NEW: DECONSTRUCT ACTION ---
    // Triggered by the "Deconstruct" action in your Input Action Asset
    public void OnDeconstruct(InputValue value)
    {
        if (!value.isPressed) return;

        // 1. Get Mouse Position and convert to World Space
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
        worldPos.z = 0;

        // 2. Detect if there is a building at that exact point
        Collider2D hit = Physics2D.OverlapPoint(worldPos, interactableLayer);

        if (hit != null)
        {
            // 3. Try to find the health script to trigger pickup
            if (hit.TryGetComponent(out objectHealth health))
            {
                Debug.Log($"<color=orange>[Deconstruct]</color> Picking up: {hit.name}");
                health.Deconstruct();
            }
            else
            {
                Debug.LogWarning("<color=yellow>[Deconstruct]</color> Object found, but it has no objectHealth script!");
            }
        }
    }

    // --- QUICK ADD WOOD (Press 1) ---
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
            Debug.LogWarning("<color=red>[Error]</color> Cannot add wood. Check assignments in Inspector!");
        }
    }

    // --- INTERACT (Press E) ---
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRadius, interactableLayer);

        if (hit != null)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact(inventory);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}