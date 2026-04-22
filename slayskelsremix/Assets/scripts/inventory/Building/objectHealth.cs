using UnityEngine;

public class objectHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Interaction")]
    public float interactionRange = 3f;

    private Transform player;

    void Awake()
    {
        currentHealth = maxHealth;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    // ---------------- DAMAGE ----------------

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // ---------------- RANGE CHECK (FIXED ERROR) ----------------

    public bool IsPlayerInRange()
    {
        if (player == null) return true;

        return Vector2.Distance(transform.position, player.position) <= interactionRange;
    }

    // ---------------- DECONSTRUCT ----------------

    public void Deconstruct()
    {
        Debug.Log($"[DECON] {gameObject.name}");

        ChestInventory chest = GetComponent<ChestInventory>();
        if (chest != null && !chest.IsEmpty())
        {
            Debug.LogWarning("[DECON] Chest not empty");
            return;
        }

        BuildIdentity id = GetComponent<BuildIdentity>();

        if (id == null || id.item == null)
        {
            Debug.LogError("[DECON] Missing BuildIdentity or item");
            return;
        }

        InventoryManager.Instance?.AddItem(id.item, 1);

        Destroy(gameObject);
    }

    // ---------------- DEATH ----------------

    void Die()
    {
        Debug.Log($"[DEATH] {gameObject.name}");

        Destroy(gameObject);
    }
}