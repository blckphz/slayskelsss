using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
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

    // 🔥 REQUIRED BY INTERFACE
    public void OnFocus()
    {
        highlight?.SetHighlighted(true);
    }

    public void OnLoseFocus()
    {
        highlight?.SetHighlighted(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!autoPickupOnCollision) return;

        if (collision.TryGetComponent(out InventoryManager playerInv))
        {
            playerInv.AddItem(woodData, amount);
            Destroy(gameObject);
            return;
        }

        if (collision.TryGetComponent(out NpcInvBrain npcInv))
        {
            npcInv.AddItem(woodData, amount);
            NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());
            Destroy(gameObject);
        }
    }
}