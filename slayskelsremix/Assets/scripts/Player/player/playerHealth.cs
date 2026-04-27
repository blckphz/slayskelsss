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
            UpdateUI();
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount); // Subtract health
        lastDamageTime = Time.time;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
    }

    protected override void Die()
    {
        Debug.LogError("playerHealth: PLAYER HAS DIED.");
        // Logic for game over screen or respawning
    }
}