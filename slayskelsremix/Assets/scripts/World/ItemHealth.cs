using System;
using System.Collections;
using UnityEngine;

public abstract class ItemHealth : MonoBehaviour, IDamageable
{
    [Header("Persistent ID")]
    public string UniqOverworldItemID;
    public string itemName;

    [Header("Health")]
    public float health = 50f;
    protected float maxHealth;

    [Header("Tool Requirements")]
    public ToolType effectiveTool = ToolType.None;

    [Header("Tool Durability Cost")]
    public float durabilityPerHit = 1f;

    [Header("XP Reward")]
    public bool givesXP = true;
    public int xpReward = 15;

    [Header("Loot")]
    public GameObject lootPrefab;
    public int dropAmount = 3;

    [Header("Hit Loot")]
    [Range(0f, 1f)]
    public float hitDropChance = 0.2f;
    public int hitDropAmount = 1;

    [Header("Damage UI")]
    public GameObject damageTextPrefab;

    [Header("Flash")]
    public string hitIntensityName = "_Intensity";
    public float flashDuration = 0.15f;

    [Header("Shake")]
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;

    [Header("Camera Shake")]
    public float cameraShakeIntensity = 1.5f;
    public float cameraShakeDuration = 0.15f;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip deathSFX;
    [Range(0f, 1f)] public float hitVolume = 1f;

    protected SpriteRenderer spriteRenderer;
    protected MaterialPropertyBlock propertyBlock;

    protected Coroutine flashCoroutine;
    protected Coroutine shakeCoroutine;

    protected Vector3 originalLocalPosition;

    private bool isSlowed = false;

    // =========================
    // INIT
    // =========================

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        originalLocalPosition = transform.localPosition;

        maxHealth = health;

        // ❗ IMPORTANT: NO ID generation here anymore
    }

    // =========================
    // ID SYSTEM (HYBRID SAFE)
    // =========================

    public void EnsureID(bool hasSaveData)
    {
        if (!string.IsNullOrEmpty(UniqOverworldItemID))
            return;

        if (hasSaveData)
            return;

        GenerateUniqueIDIfNeeded();
    }

    protected void GenerateUniqueIDIfNeeded()
    {
        Vector3 pos = transform.position;

        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);

        UniqOverworldItemID = $"{itemName}_{x}_{y}";
        Debug.Log(UniqOverworldItemID);
    }

    // =========================
    // DAMAGE
    // =========================

    protected virtual bool IsDamageIgnored() => false;

    public virtual void TakeDamage(int damage, ToolType toolType, ItemData toolItem)
    {
        if (IsDamageIgnored()) return;

        ApplyToolDurability(toolItem);
        ProcessDamage(damage, toolType);
    }

    public virtual void TakeDamage(int damage, ToolType toolType)
    {
        if (IsDamageIgnored()) return;

        ProcessDamage(damage, toolType);
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDamageIgnored()) return;

        ProcessDamage(damage, ToolType.None);
    }

    protected void ProcessDamage(int damage, ToolType toolType)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        AudioManager.Instance?.PlaySound(hitSound, hitVolume);
        CameraShaker.Instance?.Shake(cameraShakeIntensity, cameraShakeDuration);

        ShowDamageText(damage);
        TriggerFlash();
        TriggerShake();

        if (UnityEngine.Random.value <= hitDropChance)
            SpawnHitLoot();

        if (health <= 0)
            Die(toolType);
    }

    protected virtual void ApplyToolDurability(ItemData toolItem)
    {
        if (toolItem == null) return;
        if (toolItem.IsUnbreakable) return;
        if (PlayerHotbarManager.Instance == null) return;

        PlayerHotbarManager.Instance.ReduceActiveToolDurability(
            Mathf.CeilToInt(durabilityPerHit));
    }

    protected void SpawnHitLoot()
    {
        if (lootPrefab == null) return;

        for (int i = 0; i < hitDropAmount; i++)
        {
            GameObject loot = Instantiate(lootPrefab, transform.position, Quaternion.identity);

            if (loot.TryGetComponent(out LootArc arc))
                arc.Initialize(transform.position);
        }
    }

    public virtual void SpawnLoot()
    {
        if (lootPrefab == null) return;

        for (int i = 0; i < dropAmount; i++)
        {
            GameObject loot = Instantiate(lootPrefab, transform.position, Quaternion.identity);

            if (loot.TryGetComponent(out LootArc arc))
                arc.Initialize(transform.position);
        }
    }

    protected void ShowDamageText(int damage)
    {
        if (damageTextPrefab == null || damage <= 0) return;

        GameObject obj = Instantiate(damageTextPrefab, transform.position + Vector3.up, Quaternion.identity);

        if (obj.TryGetComponent(out DamageNumber dn))
            dn.Setup(damage);
    }

    // =========================
    // FLASH
    // =========================

    protected void TriggerFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float t = 0f;

        while (t < flashDuration)
        {
            t += Time.deltaTime;

            float intensity = Mathf.Lerp(1f, 0f, t / flashDuration);

            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(hitIntensityName, intensity);
            spriteRenderer.SetPropertyBlock(propertyBlock);

            yield return null;
        }

        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(hitIntensityName, 0f);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    // =========================
    // SHAKE
    // =========================

    protected void TriggerShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            Vector2 offset = UnityEngine.Random.insideUnitCircle * shakeMagnitude;
            transform.localPosition = originalLocalPosition + (Vector3)offset;

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
    }

    // =========================
    // XP + DEATH
    // =========================

    protected virtual void GrantXP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && player.TryGetComponent(out LevelManager lm))
            lm.AddXP(xpReward);
    }

    protected virtual void Die(ToolType tool)
    {
        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());

        if (givesXP)
            GrantXP();

        SpawnLoot();

        Destroy(gameObject);
    }

    // =========================
    // SLOW (INTERFACE FIX)
    // =========================

    public virtual void ApplySlow(float slowPercent, float duration, int tickDmg, float tickInterval)
    {
        StartCoroutine(SlowRoutine(duration));
    }

    private IEnumerator SlowRoutine(float duration)
    {
        isSlowed = true;
        yield return new WaitForSeconds(duration);
        isSlowed = false;
    }

    public bool IsSlowed => isSlowed;
}