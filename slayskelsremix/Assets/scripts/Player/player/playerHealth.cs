using UnityEngine;
using UnityEngine.UI;

public class playerHealth : healthMaster
{
    [Header("Regen Settings")]
    public float healthRegenRate;
    public float healthRegenDelay;

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
            currentHealth += Mathf.RoundToInt(healthRegenRate * Time.deltaTime);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateUI();
        }

        // Keep slider max value in sync if maxHealth changes (e.g., after leveling up)
        if (healthSlider != null && healthSlider.maxValue != maxHealth)
        {
            healthSlider.maxValue = maxHealth;
        }
    }

    // Updated to match healthMaster signature
    public override void TakeDamage(int amount, GameObject attacker = null)
    {
        // Passes the damage and the attacker to the base healthMaster logic
        base.TakeDamage(amount, attacker);

        lastDamageTime = Time.time;
        UpdateUI();
    }

    // A dedicated Heal method that forces a UI update
    public void Heal(int amount)
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

        GameObject gameOverCanvas = GameObject.Find("gameoverFam");

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("GameOverCanvas not found in scene!");
        }
    }
}