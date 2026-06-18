using System;
using System.Collections;
using UnityEngine;

public abstract class ItemHealth : MonoBehaviour, IDamageable
{
    [Header("Persistent ID")]
    public string UniqOverworldItemID;

    [Header("Health")]
    public float health = 50f;
    protected float maxHealth;

    [Header("Tool Requirements")]
    public ToolType effectiveTool = ToolType.None;

    [Header("Tool Effectiveness Bonus")]
    public float correctToolDamageMultiplier = 1.5f;

    [Header("Tool Durability Cost")]
    public float durabilityPerHit = 1f;

    [Header("XP Reward")]
    public bool givesXP = true;
    public int xpReward = 15;

    [Header("Loot")]
    public GameObject lootPrefab;
    public int dropAmount = 3;
    public int correctToolUsageBonus;

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

    [Header("Hit Audio")]
    public AudioClip hitSound;
    public AudioClip deathSFX;
    [Range(0f, 1f)] public float hitVolume = 1f;

    protected SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private Coroutine flashCoroutine;
    private Coroutine shakeCoroutine;

    private Vector3 originalLocalPosition;

    private bool isSlowed = false;

    // =====================================================
    // UNITY
    // =====================================================

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        originalLocalPosition = transform.localPosition;

        maxHealth = health;

    }

    protected virtual void Start()
    {
        GenerateUniqueIDIfNeeded();
    }

    private void GenerateUniqueIDIfNeeded()
    {
        UniqOverworldItemID = Guid.NewGuid().ToString();
    }

    protected virtual bool IsDamageIgnored()
    {
        return false;
    }

    // =====================================================
    // DAMAGE
    // =====================================================

    public virtual void TakeDamage(int damage, ToolType toolType, ItemData toolItem)
    {
        if (IsDamageIgnored())
        {
            Debug.Log("[ItemHealth] Damage ignored (custom override)");
            return;
        }

        ApplyToolDurability(toolItem);
        ProcessDamage(damage, toolType);
    }

    public virtual void TakeDamage(int damage, ToolType toolType)
    {
        if (IsDamageIgnored())
        {
            Debug.Log("[ItemHealth] Damage ignored (toolType overload)");
            return;
        }

        ProcessDamage(damage, toolType);
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDamageIgnored())
        {
            Debug.Log("[ItemHealth] Damage ignored (basic overload)");
            return;
        }

        ProcessDamage(damage, ToolType.None);
    }

    private void ProcessDamage(int damage, ToolType toolType)
    {
        bool correctToolUsed =
            (toolType == effectiveTool) &&
            effectiveTool != ToolType.None;

        if (correctToolUsed)
        {
            float original = damage;
            damage = Mathf.RoundToInt(damage * correctToolDamageMultiplier);

        }

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);


        AudioManager.Instance?.PlaySound(hitSound, hitVolume);

        CameraShaker.Instance?.Shake(
            cameraShakeIntensity,
            cameraShakeDuration
        );

        ShowDamageText(damage);
        TriggerFlash();
        TriggerShake();

        if (UnityEngine.Random.value <= hitDropChance)
        {
            Debug.Log("[ItemHealth] Hit loot spawned.");
            SpawnHitLoot();
        }

        if (health <= 0)
        {
            Debug.Log("[ItemHealth] Object destroyed → Die()");
            Die(toolType);
        }
    }

    // =====================================================
    // TOOL DURABILITY
    // =====================================================

    protected virtual void ApplyToolDurability(ItemData toolItem)
    {
        if (toolItem == null)
        {
            Debug.Log("[ItemHealth] No tool item provided.");
            return;
        }

        if (toolItem.IsUnbreakable)
        {
            Debug.Log("[ItemHealth] Tool is unbreakable.");
            return;
        }

        if (PlayerHotbarManager.Instance == null)
        {
            Debug.LogWarning("[ItemHealth] No PlayerHotbarManager instance.");
            return;
        }

        PlayerHotbarManager.Instance.ReduceActiveToolDurability(
            Mathf.CeilToInt(durabilityPerHit));

    }

    // =====================================================
    // LOOT
    // =====================================================

    protected virtual void SpawnHitLoot()
    {
        if (lootPrefab == null)
            return;

        for (int i = 0; i < hitDropAmount; i++)
        {
            GameObject loot = Instantiate(
                lootPrefab,
                transform.position,
                Quaternion.identity);

            if (loot.TryGetComponent(out LootArc arc))
            {
                arc.Initialize(transform.position);
            }
        }
    }

    public virtual void SpawnLoot()
    {
        if (lootPrefab == null)
            return;

        for (int i = 0; i < dropAmount; i++)
        {
            GameObject loot = Instantiate(
                lootPrefab,
                transform.position,
                Quaternion.identity);

            if (loot.TryGetComponent(out LootArc arc))
            {
                arc.Initialize(transform.position);
            }
        }
    }

    // =====================================================
    // DAMAGE UI
    // =====================================================

    protected virtual void ShowDamageText(int damage)
    {
        if (damageTextPrefab == null || damage <= 0)
            return;

        GameObject obj = Instantiate(
            damageTextPrefab,
            transform.position + Vector3.up,
            Quaternion.identity);

        if (obj.TryGetComponent(out DamageNumber dn))
        {
            dn.Setup(damage);
        }
    }

    // =====================================================
    // EFFECTS
    // =====================================================

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

    // =====================================================
    // XP
    // =====================================================

    protected virtual void GrantXP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && player.TryGetComponent(out LevelManager lm))
        {
            lm.AddXP(xpReward);
            Debug.Log($"[ItemHealth] Granted {xpReward} XP");
        }
    }

    // =====================================================
    // DEATH
    // =====================================================

    protected virtual void Die(ToolType tool)
    {
        Debug.Log($"[ItemHealth] Die() called by tool: {tool}");

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

    // =====================================================
    // SLOW
    // =====================================================

    public virtual void ApplySlow(float slowPercent, float duration, int tickDmg, float tickInterval)
    {
        StartCoroutine(SlowRoutine(duration));
        Debug.Log($"[ItemHealth] Slow applied: {slowPercent}% for {duration}s");
    }

    private IEnumerator SlowRoutine(float duration)
    {
        isSlowed = true;
        yield return new WaitForSeconds(duration);
        isSlowed = false;

        Debug.Log("[ItemHealth] Slow ended");
    }

    public bool IsSlowed => isSlowed;
}