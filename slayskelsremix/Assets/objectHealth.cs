using UnityEngine;

public class objectHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 50f;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        Debug.Log($"[{gameObject.name}] Took {amount} damage. HP: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[{gameObject.name}] Destroyed!");

        Destroy(gameObject);
    }
}