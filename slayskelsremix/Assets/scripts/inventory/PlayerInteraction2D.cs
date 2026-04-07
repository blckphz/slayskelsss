using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction2D : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRadius = 1.5f;
    public LayerMask interactableLayer;
    public InventoryManager inventory;

    [Header("Quick Add (Press 1)")]
    public ItemData woodItemData;

    // --- DECONSTRUCT (RIGHT CLICK) ---
    public void OnDeconstruct(InputValue value)
    {
        if (!value.isPressed)
        {
            Debug.Log("<color=grey>[INPUT]</color> Deconstruct released");
            return;
        }

        Debug.Log("<color=cyan>[INPUT]</color> Deconstruct pressed");

        // 1. Get mouse position
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Debug.Log($"<color=cyan>[MOUSE]</color> Screen Pos: {mousePos}");

        // 2. Convert to world
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        Debug.Log($"<color=cyan>[MOUSE]</color> World Pos: {worldPos}");

        // 3. Check for collider at that exact point
        Collider2D hit = Physics2D.OverlapPoint(worldPos, interactableLayer);

        if (hit == null)
        {
            Debug.Log("<color=red>[DECONSTRUCT]</color> No object under mouse");
            return;
        }

        Debug.Log($"<color=green>[DECONSTRUCT]</color> Hit: {hit.name}");

        // 4. Check for objectHealth
        if (hit.TryGetComponent(out objectHealth health))
        {
            Debug.Log($"<color=orange>[DECONSTRUCT]</color> Deconstructing {hit.name}");
            health.Deconstruct();
        }
    }

    // --- QUICK ADD WOOD ---
    public void OnAddWood(InputValue value)
    {
        if (!value.isPressed) return;

        if (inventory != null && woodItemData != null)
        {
            Debug.Log("<color=cyan>[HOTBAR]</color> Adding 1 Wood");
            inventory.AddItem(woodItemData, 1);
        }
        else
        {
            Debug.LogWarning("<color=red>[ERROR]</color> Inventory or WoodItem missing!");
        }
    }

    // --- INTERACT (E) ---
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        Debug.Log("<color=cyan>[INPUT]</color> Interact pressed");

        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRadius, interactableLayer);

        if (hit == null)
        {
            Debug.Log("<color=red>[INTERACT]</color> Nothing nearby");
            return;
        }

        Debug.Log($"<color=green>[INTERACT]</color> Found: {hit.name}");

        if (hit.TryGetComponent(out IInteractable interactable))
        {
            interactable.Interact(inventory);
        }
        else
        {
            Debug.LogWarning("<color=yellow>[INTERACT]</color> No IInteractable on object");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}