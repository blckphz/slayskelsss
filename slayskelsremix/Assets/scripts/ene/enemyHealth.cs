using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Pathfinding;

public class enemyHealth : healthMaster, IDamageable
{
    [Header("Loot Settings")]
    public GameObject goldPrefab;
    public int minGold = 1;
    public int maxGold = 3;

    [Header("UI & Effects")]
    public GameObject healthBarObject;
    public Image healthBarFill;
    public GameObject damageTextPrefab;
    public float flashDuration = 0.2f;

    [Header("Shader Property Names")]
    public string hitIntensityName = "_Intensity";           // Generic Hit Flash
    public string stunIntensityName = "_StunIntensity";       // Thunder/Stun Effect
    public string berrySmearIntensityName = "_BerrySmearIntensity"; // Berry Juice Effect

    [Header("Berry Smear Stacking (Uncapped)")]
    public float smearDecaySpeed = 0.5f;
    private float currentSmearIntensity = 0f;

    private AIPath ai;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private comboScript comboSys;

    private float originalSpeed;
    private Coroutine _flashCoroutine;
    private Coroutine _slowCoroutine;
    private Coroutine _tickDamageCoroutine;

    public bool IsSlowed => _slowCoroutine != null;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ai = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        if (ai != null) originalSpeed = ai.maxSpeed;
        comboSys = Object.FindAnyObjectByType<comboScript>();
        UpdateHealthUI();
    }

    void Update()
    {
        // Decay the smear over time
        if (currentSmearIntensity > 0)
        {
            currentSmearIntensity -= smearDecaySpeed * Time.deltaTime;

            // Ensure we don't go below zero
            if (currentSmearIntensity < 0) currentSmearIntensity = 0;

            UpdateShaderFloat(berrySmearIntensityName, currentSmearIntensity);
        }
    }

    // --- IDAMAGEABLE & OVERRIDES ---

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        UpdateHealthUI();
        ShowDamageText(damage);

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashEffect());
    }

    // --- THUNDER / STUN LOGIC ---
    public void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval)
    {
        if (_slowCoroutine != null) StopCoroutine(_slowCoroutine);
        _slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));

        if (_tickDamageCoroutine != null) StopCoroutine(_tickDamageCoroutine);
        _tickDamageCoroutine = StartCoroutine(TickDamageRoutine(tickDmg, tickInterval, duration));
    }

    // --- BERRY SMEAR LOGIC (Uncapped Stacking) ---
    public void AddSmear(float amount)
    {
        currentSmearIntensity += amount;
        // No Clamp here! The intensity can go as high as you want.
        UpdateShaderFloat(berrySmearIntensityName, currentSmearIntensity);

        Debug.Log($"[EnemyHealth] Smear Intensity is now: {currentSmearIntensity}");
    }

    protected override void Die()
    {
        UpdateShaderFloat(hitIntensityName, 0f);
        UpdateShaderFloat(stunIntensityName, 0f);
        UpdateShaderFloat(berrySmearIntensityName, 0f);

        if (comboSys != null) comboSys.RegisterKill();
        SpawnLoot();
        base.Die();
    }

    // --- COROUTINES ---

    public void ApplyImpulse(Vector2 force)
    {
        if (rb == null) return;
        if (ai != null) ai.canMove = false;
        rb.AddForce(force, ForceMode2D.Impulse);
        StopCoroutine(nameof(ResetAI));
        StartCoroutine(ResetAI());
    }

    private IEnumerator ResetAI()
    {
        yield return new WaitForSeconds(0.3f);
        if (ai != null) ai.canMove = true;
    }

    private IEnumerator FlashEffect()
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

    private IEnumerator SlowRoutine(float slowAmount, float duration)
    {
        if (ai != null)
        {
            ai.maxSpeed = Mathf.Max(0.1f, originalSpeed - slowAmount);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pulse = 0.85f + Mathf.Sin(Time.time * 15f) * 0.35f;
                UpdateShaderFloat(stunIntensityName, pulse);
                yield return null;
            }
            ai.maxSpeed = originalSpeed;
            UpdateShaderFloat(stunIntensityName, 0f);
        }
        _slowCoroutine = null;
    }

    private IEnumerator TickDamageRoutine(float dmg, float interval, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;
            TakeDamage(dmg);
        }
        _tickDamageCoroutine = null;
    }

    private void UpdateShaderFloat(string name, float value)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(name, value);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private void SpawnLoot()
    {
        if (goldPrefab == null) return;
        int amount = Random.Range(minGold, maxGold + 1);
        for (int i = 0; i < amount; i++)
        {
            Instantiate(goldPrefab, transform.position, Quaternion.identity);
        }
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null) healthBarFill.fillAmount = currentHealth / maxHealth;
        if (healthBarObject != null) healthBarObject.SetActive(currentHealth < maxHealth);
    }

    void ShowDamageText(float damage)
    {
        if (damageTextPrefab != null && damage > 0)
        {
            GameObject textObj = Instantiate(damageTextPrefab, transform.position + Vector3.up, Quaternion.identity);
            if (textObj.TryGetComponent<DamageNumber>(out DamageNumber dn)) dn.Setup(damage);
        }
    }
}