using UnityEngine;

public class healthMaster : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("XP Reward")]
    public int xpReward = 20;
    public bool givesXP = true;

    protected GameObject lastAttacker;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount, GameObject attacker = null)
    {
        if (attacker != null) lastAttacker = attacker;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0f) Die();
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