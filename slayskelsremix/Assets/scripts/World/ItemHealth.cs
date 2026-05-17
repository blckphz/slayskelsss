using UnityEngine;
using System.Collections;

public abstract class ItemHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float health = 50f;
    protected float maxHealth;

    [Header("XP Reward")]
    public bool givesXP = true;
    public int xpReward = 15;

    [Header("Loot (Base)")]
    public GameObject lootPrefab;
    public int dropAmount = 3;

    [Header("Damage UI")]
    public GameObject damageTextPrefab;

    [Header("Flash Effect")]
    public string hitIntensityName = "_Intensity";
    public float flashDuration = 0.15f;

    [Header("Shake Effect")]
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;

    protected SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private Coroutine flashCoroutine;
    private Coroutine shakeCoroutine;

    private Vector3 originalLocalPosition;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        originalLocalPosition = transform.localPosition;
        maxHealth = health;

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[ItemHealth] {gameObject.name} is missing a SpriteRenderer!", gameObject);
        }
    }

    public virtual void TakeDamage(float damage, ToolType toolType)
    {
        Debug.Log($"[ItemHealth] {gameObject.name} hit by ToolType: {toolType} with initial damage: {damage}");
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        Debug.Log($"[ItemHealth] {gameObject.name} processing standard damage. Current Health: {health}/{maxHealth}, Incoming Damage: {damage}");

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        ShowDamageText(damage);
        TriggerFlash();
        TriggerShake();

        if (health <= 0)
        {
            Debug.Log($"[ItemHealth] {gameObject.name} health reached 0. Triggering Die().");
            Die();
        }
    }

    public virtual void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval)
    {
        // Default: Do nothing (Static items like trees don't move)
    }

    public bool IsSlowed => false;

    protected virtual void GrantXP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.TryGetComponent<LevelManager>(out LevelManager lm))
            {
                Debug.Log($"[ItemHealth] Granting {xpReward} XP to player from {gameObject.name}.");
                lm.AddXP(xpReward);
            }
            else
            {
                Debug.LogWarning($"[ItemHealth] Player found, but LevelManager component is missing!", player);
            }
        }
        else
        {
            Debug.LogWarning($"[ItemHealth] Unable to grant XP! No GameObject found with tag 'Player'.");
        }
    }

    protected void TriggerFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float intensity = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            UpdateShaderFloat(hitIntensityName, intensity);
            yield return null;
        }

        UpdateShaderFloat(hitIntensityName, 0f);
    }

    protected void UpdateShaderFloat(string name, float value)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(name, value);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    protected void TriggerShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            Vector2 randomOffset = Random.insideUnitCircle * shakeMagnitude;
            transform.localPosition = originalLocalPosition + (Vector3)randomOffset;

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
    }

    protected virtual void Die()
    {
        Debug.Log($"[ItemHealth] {gameObject.name} has died. Notifying systems and executing cleanup.");
        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());
        UpdateShaderFloat(hitIntensityName, 0f);

        if (givesXP)
        {
            GrantXP();
        }

        SpawnLoot();
        Destroy(gameObject);
    }

    public virtual void SpawnLoot()
    {
        if (lootPrefab == null)
        {
            Debug.LogWarning($"[ItemHealth] {gameObject.name} has no lootPrefab assigned.", gameObject);
            return;
        }

        Debug.Log($"[ItemHealth] Spawning base loot for {gameObject.name}. Count: {dropAmount}");
        for (int i = 0; i < dropAmount; i++)
        {
            GameObject loot = Instantiate(lootPrefab, transform.position, Quaternion.identity);

            LootArc arc = loot.GetComponent<LootArc>();
            if (arc != null)
            {
                arc.Initialize(transform.position);
            }
        }
    }

    protected void ShowDamageText(float damage)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning($"[ItemHealth] {gameObject.name} is missing a damageTextPrefab assignment.", gameObject);
            return;
        }

        if (damage > 0)
        {
            GameObject textObj = Instantiate(
                damageTextPrefab,
                transform.position + Vector3.up,
                Quaternion.identity
            );

            if (textObj.TryGetComponent<DamageNumber>(out DamageNumber dn))
            {
                dn.Setup(damage);
            }
        }
    }
}