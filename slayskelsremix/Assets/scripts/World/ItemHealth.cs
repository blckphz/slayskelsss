using UnityEngine;
using System.Collections;

// "abstract" means this script is a template and cannot be attached to a GameObject directly.
// You must create a subclass (like treeItemBehav) to use it.
public abstract class ItemHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float health = 50f;
    protected float maxHealth;

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
    }

    // "virtual" allows child scripts to use "override" to add their own logic.
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        ShowDamageText(damage);
        TriggerFlash();
        TriggerShake();

        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval)
    {
        // Default: Do nothing (Static items don't move)
    }

    public bool IsSlowed => false;

    // ---------------- FLASH ----------------

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

    // ---------------- SHAKE ----------------

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

    // ---------------- DEATH ----------------

    // Trees use a fall sequence, but a crate might just explode. 
    // Making this virtual let's us change the behavior per item.
    protected virtual void Die()
    {
        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());
        UpdateShaderFloat(hitIntensityName, 0f);
        SpawnLoot();
        Destroy(gameObject);
    }

    public virtual void SpawnLoot()
    {
        if (lootPrefab == null) return;

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

    // ---------------- UI ----------------

    protected void ShowDamageText(float damage)
    {
        if (damageTextPrefab != null && damage > 0)
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