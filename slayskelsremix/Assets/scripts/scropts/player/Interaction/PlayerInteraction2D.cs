using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction2D : MonoBehaviour
{
    [Header("Settings")]
    public float interactRadius = 2f;
    public LayerMask interactableLayer;
    public InventoryManager inventory;

    [Header("Performance")]
    [SerializeField] private float scanInterval = 0.1f;
    [SerializeField] private int maxResults = 16;

    [Header("Input")]
    public InputActionReference interactAction;

    private IInteractable currentInteractable;

    private Collider2D[] results;
    private float scanTimer;

    private void Awake()
    {
        results = new Collider2D[maxResults];

        if (inventory == null)
            Debug.LogError("[INIT] Inventory is NOT assigned!");
    }

    private void OnEnable()
    {
        if (interactAction == null)
        {
            Debug.LogError("[INPUT] interactAction NOT assigned!");
            return;
        }

        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    private void Update()
    {
        DetectNearest();

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void DetectNearest()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer < scanInterval)
            return;

        scanTimer = 0f;

        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            interactRadius,
            results,
            interactableLayer
        );

        IInteractable nearest = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = results[i];
            if (hit == null) continue;

            if (hit.TryGetComponent(out IInteractable interactable))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = interactable;
                }
            }
        }

        if (nearest != currentInteractable)
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnLoseFocus();
                InteractionUI.Instance?.Hide();

                // 🔹 AUTO-CLOSE CAMPFIRE
                if (currentInteractable is CampfireBehav)
                {
                    CampfireUI.Instance?.CloseCampfire();
                }

                // 🔹 AUTO-CLOSE CHEST
                // We check if it's a ChestInventory and close it if the player walks away
                if (currentInteractable is ChestInventory chest)
                {
                    chest.CloseChest();
                }
            }

            currentInteractable = nearest;

            if (currentInteractable != null)
            {
                currentInteractable.OnFocus();

                string key = GetBoundKey();
                InteractionUI.Instance?.Show($"[{key}]");
            }
        }
    }

    private string GetBoundKey()
    {
        if (interactAction == null || interactAction.action == null)
            return "E";

        return interactAction.action.bindings[0].ToDisplayString();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        TryInteract();
    }

    private void TryInteract()
    {
        if (currentInteractable == null) return;
        if (inventory == null) return;

        currentInteractable.Interact(inventory);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}