using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemHealth : MonoBehaviour, IDamageable
{
    public string UniqOverworldItemID;

    [Header("Health")]
    public float health = 50f;
    protected float maxHealth;

    [Header("Tool Requirements")]
    public ToolType effectiveTool = ToolType.None;

    [Header("Tool Durability")]
    public float durabilityPerSwing = 1f;

    [Header("XP Reward")]
    public bool givesXP = true;
    public int xpReward = 15;

    [Header("Loot (Base)")]
    public GameObject lootPrefab;
    public int dropAmount = 3;
    public int correctToolUsageBonus;

    [Header("Bonus Drop On Hit")]
    [Range(0f, 1f)]
    public float hitDropChance = 0.2f;   // 20% chance
    public int hitDropAmount = 1;

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
    }

    // ================= DAMAGE =================

    public virtual void TakeDamage(float damage, ToolType toolType, ItemData toolItem)
    {
        LogToolDurability(toolItem);

        if (toolItem != null &&
            !toolItem.IsUnbreakable &&
            PlayerHotbarManager.Instance != null)
        {
            float durabilityLoss = durabilityPerSwing;

            // correct tool = half durability loss
            if (toolType == effectiveTool)
            {
                durabilityLoss *= 0.5f;
            }

            // round up
            int finalLoss = Mathf.CeilToInt(durabilityLoss);

            PlayerHotbarManager.Instance.ReduceActiveToolDurability(finalLoss);
        }

        ProcessDamage(damage, toolType);
    }

    public virtual void TakeDamage(float damage, ToolType toolType)
    {
        ProcessDamage(damage, toolType);
    }

    public virtual void TakeDamage(float damage)
    {
        ProcessDamage(damage, ToolType.None);
    }

    // ================= CORE DAMAGE =================

    private void ProcessDamage(float damage, ToolType toolType)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        ShowDamageText(damage);
        TriggerFlash();
        TriggerShake();

        // 20% chance to drop loot on every hit
        if (UnityEngine.Random.value <= hitDropChance)
        {
            SpawnHitLoot();
        }

        if (health <= 0)
        {
            Die(toolType);
        }
    }

    // ================= TOOL DEBUG =================

    private void LogToolDurability(ItemData tool)
    {
        if (tool == null) return;

        if (!tool.IsUnbreakable && PlayerHotbarManager.Instance != null)
        {
            float current = PlayerHotbarManager.Instance.GetActiveToolDurability();
            float percent = (current / tool.maxDurability) * 100f;

            percent = Mathf.Clamp(percent, 0f, 100f);

          
        }
    }

    // ================= XP =================

    protected virtual void GrantXP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null &&
            player.TryGetComponent<LevelManager>(out LevelManager lm))
        {
            lm.AddXP(xpReward);
        }
    }

    // ================= DEATH =================

    protected virtual void Die(ToolType killerTool)
    {
        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());

        if (givesXP)
            GrantXP();

        SpawnLoot();

        Destroy(gameObject);
    }

    protected virtual void Die()
    {
        Die(ToolType.None);
    }

    // ================= LOOT =================

    public virtual void SpawnLoot()
    {
        if (lootPrefab == null)
            return;

        for (int i = 0; i < dropAmount; i++)
        {
            GameObject loot = Instantiate(
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

    protected virtual void SpawnHitLoot()
    {
        if (lootPrefab == null)
            return;

        for (int i = 0; i < hitDropAmount; i++)
        {
            GameObject loot = Instantiate(
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

    // ================= VISUALS =================

    protected void ShowDamageText(float damage)
    {
        if (damageTextPrefab == null || damage <= 0)
            return;

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
                UnityEngine.Random.insideUnitCircle * shakeMagnitude;

            transform.localPosition =
                originalLocalPosition + (Vector3)randomOffset;

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
    }

    public virtual void ApplySlow(
        float slowPercent,
        float duration,
        float tickDmg,
        float tickInterval)
    {
    }

    public bool IsSlowed => false;
}