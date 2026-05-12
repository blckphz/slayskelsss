using UnityEngine;

public class healthMaster : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("XP Reward")]
    public int xpReward = 20;   // how much XP this object gives when it dies
    public bool givesXP = true;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

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
        if (givesXP)
        {
            LevelManager playerLevel = FindFirstObjectByType<LevelManager>();

            if (playerLevel != null)
            {
                playerLevel.AddXP(xpReward);
            }
        }

        Destroy(gameObject);
    }
}