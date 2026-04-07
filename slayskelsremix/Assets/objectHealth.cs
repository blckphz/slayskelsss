using UnityEngine;

public class objectHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Interaction")]
    public float interactionRange = 3f;

    private Transform player;

    private void Awake()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    // ✅ DAMAGE SYSTEM (KEPT)
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        Debug.Log($"{gameObject.name} took {amount} damage. HP: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public bool IsPlayerInRange()
    {
        if (player == null) return true;

        return Vector2.Distance(transform.position, player.position) <= interactionRange;
    }

    // ✅ RIGHT CLICK ACTION (PICKUP)
    public void Deconstruct()
    {
        BuildIdentity identity = GetComponent<BuildIdentity>();

        if (identity == null || identity.item == null)
        {
            Debug.LogError($"{gameObject.name} missing BuildIdentity or item!");
            return;
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(identity.item, 1);
            Debug.Log($"Picked up {identity.item.itemName}");
        }

        Destroy(gameObject);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} destroyed");
        Destroy(gameObject);
    }
}