using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ItemHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float health = 50f;
    protected float maxHealth;

    [Header("Tool Durability")]
    public float durabilityPerSwing = 1f; // How much durability this object consumes per hit

    [Header("XP Reward")]
    public bool givesXP = true;
    public int xpReward = 15;

    [Header("Loot (Base)")]
    public GameObject lootPrefab;
    public int dropAmount = 3;
    public int correctToolUsageBonus;

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

    // ================= DAMAGE INTERFACE OVERLOADS =================

    /// <summary>
    /// Primary entry point: Processes damage, logs durability details, and decrements active tool wear.
    /// </summary>
    public virtual void TakeDamage(float damage, ToolType toolType, ItemData toolItem)
    {
        // 1. Log durability before reducing it
        LogToolDurability(toolItem);

        // 2. Reduce durability using durabilityPerSwing
        if (toolItem != null &&
            !toolItem.IsUnbreakable &&
            PlayerHotbarManager.Instance != null)
        {
            PlayerHotbarManager.Instance.ReduceActiveToolDurability(durabilityPerSwing);
        }

        // 3. Process structural damage
        ProcessDamage(damage, toolType);
    }

    /// <summary>
    /// Fallback overload with ToolType only.
    /// </summary>
    public virtual void TakeDamage(float damage, ToolType toolType)
    {
        Debug.Log($"[ItemHealth] {gameObject.name} hit by ToolType: {toolType} with initial damage: {damage}");
        ProcessDamage(damage, toolType);
    }

    /// <summary>
    /// Raw damage fallback.
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        ProcessDamage(damage, ToolType.None);
    }

    // ================= CORE CALCULATIONS =================

    private void ProcessDamage(float damage, ToolType toolType)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        ShowDamageText(damage);
        TriggerFlash();
        TriggerShake();

        if (health <= 0)
        {
            Die(toolType);
        }
    }

    private void LogToolDurability(ItemData tool)
    {
        if (tool == null) return;

        if (!tool.IsUnbreakable)
        {
            if (PlayerHotbarManager.Instance != null)
            {
                float currentRuntimeDurability =
                    PlayerHotbarManager.Instance.GetActiveToolDurability();

                float durabilityPercent =
                    (currentRuntimeDurability / tool.maxDurability) * 100f;

                durabilityPercent = Mathf.Clamp(durabilityPercent, 0f, 100f);

                Debug.Log(
                    $"<color=yellow>[Tool Check]</color> " +
                    $"{tool.itemName} Durability: " +
                    $"{currentRuntimeDurability}/{tool.maxDurability} " +
                    $"({durabilityPercent:F0}%)"
                );
            }
        }
        else
        {
            Debug.Log(
                $"<color=white>[Tool Check]</color> " +
                $"{tool.itemName} is unbreakable and does not track durability."
            );
        }
    }

    public virtual void ApplySlow(
        float slowPercent,
        float duration,
        float tickDmg,
        float tickInterval)
    {
    }

    public bool IsSlowed => false;

    // ================= REWARDS & EVENT LIFECYCLE =================

    protected virtual void GrantXP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            if (player.TryGetComponent<LevelManager>(out LevelManager lm))
            {
                Debug.Log(
                    $"[ItemHealth] Granting {xpReward} XP to player from {gameObject.name}."
                );

                lm.AddXP(xpReward);
            }
        }
    }

    protected virtual void Die(ToolType killerTool)
    {
        Debug.Log(
            $"[ItemHealth] {gameObject.name} has died. " +
            $"Notifying systems and executing cleanup."
        );

        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());

        UpdateShaderFloat(hitIntensityName, 0f);

        if (givesXP)
        {
            GrantXP();
        }

        SpawnLoot();

        Destroy(gameObject);
    }

    protected virtual void Die()
    {
        Die(ToolType.None);
    }

    public virtual void SpawnLoot()
    {
        if (lootPrefab == null)
            return;

        for (int i = 0; i < dropAmount; i++)
        {
            GameObject loot =
                Instantiate(
                    lootPrefab,
                    transform.position,
                    Quaternion.identity
                );

            if (loot.TryGetComponent<LootArc>(out LootArc arc))
            {
                arc.Initialize(transform.position);
            }
        }
    }

    protected void ShowDamageText(float damage)
    {
        if (damageTextPrefab == null || damage <= 0)
            return;

        GameObject textObj =
            Instantiate(
                damageTextPrefab,
                transform.position + Vector3.up,
                Quaternion.identity
            );

        if (textObj.TryGetComponent<DamageNumber>(out DamageNumber dn))
        {
            dn.Setup(damage);
        }
    }

    // ================= VISUAL EFFECTS =================

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

            float intensity =
                Mathf.Lerp(1f, 0f, elapsed / flashDuration);

            UpdateShaderFloat(hitIntensityName, intensity);

            yield return null;
        }

        UpdateShaderFloat(hitIntensityName, 0f);
    }

    protected void UpdateShaderFloat(string name, float value)
    {
        if (spriteRenderer == null)
            return;

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

            Vector2 randomOffset =
                Random.insideUnitCircle * shakeMagnitude;

            transform.localPosition =
                originalLocalPosition + (Vector3)randomOffset;

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
    }
}