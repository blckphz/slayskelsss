using UnityEngine;

public class ItemGiver : MonoBehaviour
{
    [Header("Settings")]
    public ItemData itemToGive;
    public int amount = 1;
    public bool destroyOnPickup = true;

    [Header("Visuals")]
    public bool bobbingEffect = true;
    private float startY;

    private void Start()
    {
        startY = transform.position.y;
    }

    private void Update()
    {
        if (bobbingEffect)
        {
            // Simple 2D floating animation
            float newY = startY + (Mathf.Sin(Time.time * 2f) * 0.1f);
            transform.position = new Vector2(transform.position.x, newY);
        }
    }

    // This is the 2D version of the trigger check
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ensure your Player object has the "Player" tag assigned in the Inspector
        if (other.CompareTag("Player"))
        {
            GiveItem();
        }
    }

    private void GiveItem()
    {
        if (itemToGive == null)
        {
            Debug.LogWarning("ItemGiver2D: Assign an ItemData ScriptableObject!");
            return;
        }

        if (InventoryManager.Instance != null)
        {
            // Adds item to your InventoryManager list and saves
            InventoryManager.Instance.AddItem(itemToGive, amount);

            Debug.Log($"Collected: {itemToGive.itemName}");

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("ItemGiver2D: InventoryManager Instance missing from scene!");
        }
    }
}