using UnityEngine;

public class objectHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // --- DECONSTRUCT: REVERT TO ITEM ---
    public void Deconstruct()
    {
        // 1. Find the Identity to see what item this object "is"
        BuildIdentity identity = GetComponent<BuildIdentity>();

        if (identity != null && identity.item != null)
        {
            Debug.Log($"<color=green>[Inventory]</color> Returning {identity.item.itemName} to bag.");

            // 2. Add the item back to the inventory/hotbar
            InventoryManager.Instance.AddItem(identity.item, 1);

            // 3. Optional: Trigger a save update
            if (BuildingSaveManager.Instance != null)
            {
                // Simple destruction is usually enough as the SaveManager 
                // will skip this object next time it iterates to save.
                BuildingSaveManager.Instance.SaveNow();
            }

            Die();
        }
        else
        {
            Debug.LogError($"[Deconstruct Error] {gameObject.name} is missing a BuildIdentity or Item reference!");
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        // Add "Poof" particles or sound effects here
        Destroy(gameObject);
    }
}