using UnityEngine;
using UnityEngine.UI;

public class playerHealth : healthMaster
{
    [Header("Regen Settings")]
    public float healthRegenRate = 5f;
    public float healthRegenDelay = 3f;

    [Header("Starvation")]
    public int starvationDamage = 1;
    public float starvationInterval = 2f;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverCanvas;

    private float lastDamageTime;
    private float starvationTimer;

    private PlayerNeeds playerNeeds;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        playerNeeds = GetComponent<PlayerNeeds>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (gameOverCanvas == null)
        {
            Debug.LogError("Game Over Canvas is NOT assigned!");
        }
        else
        {
            gameOverCanvas.SetActive(false);
        }
    }

    void Update()
    {
        // Health regeneration
        if (currentHealth < maxHealth &&
            Time.time >= lastDamageTime + healthRegenDelay)
        {
            currentHealth += Mathf.RoundToInt(healthRegenRate * Time.deltaTime);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateUI();
        }

        // Starvation damage
        if (playerNeeds != null && playerNeeds.saturation <= 0)
        {
            starvationTimer += Time.deltaTime;

            if (starvationTimer >= starvationInterval)
            {
                starvationTimer = 0f;
                TakeDamage(starvationDamage);
            }
        }
        else
        {
            starvationTimer = 0f;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public override void TakeDamage(int amount, GameObject attacker = null)
    {
        base.TakeDamage(amount, attacker);

        lastDamageTime = Time.time;

        UpdateUI();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    protected override void Die()
    {
        Debug.Log("PLAYER DIED!");

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
            Debug.Log("Game Over screen shown.");
        }
        else
        {
            Debug.LogError("Game Over Canvas is NULL!");
        }

        // Disable player controls
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        // Stop movement if using Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}