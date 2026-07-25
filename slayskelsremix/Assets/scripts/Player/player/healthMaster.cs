using UnityEngine;

public class healthMaster : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("XP Reward")]
    public int xpReward = 20;
    public bool givesXP = true;

    protected GameObject lastAttacker;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount, GameObject attacker = null)
    {
        if (attacker != null)
            lastAttacker = attacker;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (givesXP && lastAttacker != null)
        {
            if (lastAttacker.TryGetComponent<LevelManager>(out LevelManager lm))
            {
                lm.AddXP(xpReward);
            }
        }

        Destroy(gameObject);
    }
}