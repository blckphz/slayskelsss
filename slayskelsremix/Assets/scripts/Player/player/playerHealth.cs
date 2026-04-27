using UnityEngine;
using UnityEngine.UI;

public class playerHealth : healthMaster
{
    [Header("Regen Settings")]
    public float healthRegenRate = 5f;
    public float healthRegenDelay = 5f;

    [Header("UI Components")]
    public Slider healthSlider;

    private float lastDamageTime;

    void Start()
    {
        // Ensure UI matches starting health
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        // Regeneration logic
        if (currentHealth < maxHealth && Time.time >= lastDamageTime + healthRegenDelay)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateUI();
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount); // This reduces currentHealth in healthMaster
        lastDamageTime = Time.time;
        UpdateUI();
    }

    // 🔥 ADD THIS: A dedicated Heal method that forces a UI update
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
        Debug.Log($"[playerHealth] Health updated to: {currentHealth}");
    }

    public void UpdateUI()
    {
        if (healthSlider != null) 
        {
            healthSlider.value = currentHealth;
        }
    }

    protected override void Die()
    {
        Debug.LogError("playerHealth: PLAYER HAS DIED.");
    }
}