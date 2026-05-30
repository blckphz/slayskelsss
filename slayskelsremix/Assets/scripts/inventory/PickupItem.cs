using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemData woodData;
    [SerializeField] private int amount = 1;

    [Header("Pickup Mode")]
    [SerializeField] private bool autoPickupOnCollision = true;

    [Header("Magnet Settings")]
    [SerializeField] private float magnetRange = 2.5f;
    [SerializeField] private float magnetStrength = 8f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private Highlightable highlight;
    private Transform player;

    private bool isPickedUp = false;

    private void Awake()
    {
        highlight = GetComponent<Highlightable>();
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void Update()
    {
        if (isPickedUp || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= magnetRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;

            // stronger pull when closer
            float strength = 1f - (dist / magnetRange);

            transform.position += direction * magnetStrength * strength * Time.deltaTime;
        }
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

        // 🔊 play pickup sound
        if (pickupSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(pickupSound, 1f);
        }

        Destroy(gameObject);
    }
}