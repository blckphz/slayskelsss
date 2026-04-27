using UnityEngine;

public class healthMaster : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    // Virtual allows child classes to add UI/VFX logic while keeping core math here
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // Default death is just destruction
        Destroy(gameObject);
    }
}