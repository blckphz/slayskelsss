using System.Collections;
using UnityEngine;

public abstract class ItemHealth : MonoBehaviour, IDamageable
{
    public string UniqOverworldItemID;

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

    protected SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private Coroutine flashCoroutine;
    private Coroutine shakeCoroutine;
    private Vector3 originalLocalPosition;

    private bool isSlowed = false;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        originalLocalPosition = transform.localPosition;
        maxHealth = health;
    }

    protected virtual bool IsDamageIgnored()
    {
        return false;
    }

    // ================= DAMAGE =================

    public virtual void TakeDamage(float damage, ToolType toolType, ItemData toolItem)
    {
        if (IsDamageIgnored()) return;

        ApplyToolDurability(toolItem);

        ProcessDamage(damage, toolType);
    }

    public virtual void TakeDamage(float damage, ToolType toolType)
    {
        if (IsDamageIgnored()) return;

        ProcessDamage(damage, toolType);
    }

    public virtual void TakeDamage(float damage)
    {
        if (IsDamageIgnored()) return;

        ProcessDamage(damage, ToolType.None);
    }

    // ================= TOOL DURABILITY CONTROL (NOW OWNED HERE) =================

    protected virtual void ApplyToolDurability(ItemData toolItem)
    {
        if (toolItem == null) return;
        if (toolItem.IsUnbreakable) return;

        if (PlayerHotbarManager.Instance == null) return;

        PlayerHotbarManager.Instance.ReduceActiveToolDurability(
            Mathf.CeilToInt(durabilityPerHit)
        );
    }

    // ================= CORE DAMAGE =================

    private void ProcessDamage(float damage, ToolType toolType)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        ShowDamageText(damage);
        TriggerFlash();
        TriggerShake();

        if (Random.value <= hitDropChance)
            SpawnHitLoot();

        if (health <= 0)
            Die(toolType);
    }

    // ================= LOOT =================

    protected virtual void SpawnHitLoot()
    {
        if (lootPrefab == null) return;

        for (int i = 0; i < hitDropAmount; i++)
        {
            GameObject loot = Instantiate(lootPrefab, transform.position, Quaternion.identity);

            if (loot.TryGetComponent<LootArc>(out LootArc arc))
                arc.Initialize(transform.position);
        }
    }

    public virtual void SpawnLoot()
    {
        if (lootPrefab == null) return;

        for (int i = 0; i < dropAmount; i++)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity);
        }
    }

    // ================= DAMAGE UI =================

    protected virtual void ShowDamageText(float damage)
    {
        if (damageTextPrefab == null || damage <= 0) return;

        GameObject obj = Instantiate(
            damageTextPrefab,
            transform.position + Vector3.up,
            Quaternion.identity
        );

        if (obj.TryGetComponent<DamageNumber>(out DamageNumber dn))
            dn.Setup(damage);
    }

    // ================= EFFECTS =================

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

            float i = Mathf.Lerp(1f, 0f, t / flashDuration);

            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(hitIntensityName, i);
            spriteRenderer.SetPropertyBlock(propertyBlock);

            yield return null;
        }
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

            Vector2 offset = Random.insideUnitCircle * shakeMagnitude;
            transform.localPosition = originalLocalPosition + (Vector3)offset;

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
    }

    // ================= XP =================

    protected virtual void GrantXP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && player.TryGetComponent(out LevelManager lm))
            lm.AddXP(xpReward);
    }

    // ================= DEATH =================

    protected virtual void Die(ToolType tool)
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

    // ================= SLOW =================

    public virtual void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval)
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