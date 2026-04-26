using UnityEngine;

public class objectHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    public float currentHealth;

    [Header("Interaction")]
    public float interactionRange = 3f;
    private Transform player;

    void Awake()
    {
        currentHealth = maxHealth;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // 🔥 FIXED: Added this back for InteractionManager
    public bool IsPlayerInRange()
    {
        if (player == null) return true;
        return Vector2.Distance(transform.position, player.position) <= interactionRange;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f) Die();
    }

    public void Deconstruct()
    {
        BuildingSaveManager.Instance?.UnregisterBuilding(gameObject);

        BuildIdentity id = GetComponent<BuildIdentity>();
        if (id != null && id.item != null)
        {
            InventoryManager.Instance?.AddItem(id.item, 1);
        }

        Destroy(gameObject);
        BuildingSaveManager.Instance?.SaveNow();
    }

    void Die()
    {
        BuildingSaveManager.Instance?.UnregisterBuilding(gameObject);
        Destroy(gameObject);
        BuildingSaveManager.Instance?.SaveNow();
    }
}