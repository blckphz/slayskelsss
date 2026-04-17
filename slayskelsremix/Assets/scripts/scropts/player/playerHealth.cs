using UnityEngine;
using UnityEngine.UI; // Needed for the Slider

public class playerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Regen Settings")]
    public float healthRegenRate = 5f;
    public float healthRegenDelay = 5f;

    [Header("UI Components")]
    public Slider healthSlider; // Drag your slider here in the inspector

    private float lastDamageTime;

    void Start()
    {
        currentHealth = maxHealth;

        // Setup slider values
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

    }

    void Update()
    {
        if (currentHealth < maxHealth && Time.time >= lastDamageTime + healthRegenDelay)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateUI(); // Update slider while regenerating
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        lastDamageTime = Time.time;

        UpdateUI(); // Update slider when taking damage


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Helper method to keep UI in sync
    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    void Die()
    {
        Debug.LogError("playerHealth: PLAYER HAS DIED.");
    }
}