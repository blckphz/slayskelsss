using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Pathfinding;

public class enemyHealth : healthMaster, IDamageable
{
    [Header("SAVE SYSTEM")]
    public string enemyID;

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
    public string hitIntensityName = "_Intensity";
    public string stunIntensityName = "_StunIntensity";
    public string berrySmearIntensityName = "_BerrySmearIntensity";

    [Header("Berry Smear")]
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

    // =========================
    // SAVE ACCESS
    // =========================
    public float GetHealth() => currentHealth;

    public void SetHealth(float value)
    {
        currentHealth = value;
        UpdateHealthUI();
    }

    // =========================
    // AWAKEN
    // =========================
    protected override void Awake()
    {
        base.Awake(); // Calls healthMaster.Awake() to set currentHealth = maxHealth

        spriteRenderer = GetComponent<SpriteRenderer>();
        ai = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
        propertyBlock = new MaterialPropertyBlock();

        if (string.IsNullOrEmpty(enemyID))
        {
            enemyID = System.Guid.NewGuid().ToString();
        }
    }

    void Start()
    {
        if (ai != null) originalSpeed = ai.maxSpeed;
        comboSys = Object.FindAnyObjectByType<comboScript>();
        UpdateHealthUI();
    }

    // =========================
    // DAMAGE
    // =========================

    // Overriding the base TakeDamage to include UI and FX
    public override void TakeDamage(float damage, GameObject attacker = null)
    {
        // Call the healthMaster logic (handles lastAttacker and currentHealth)
        base.TakeDamage(damage, attacker);

        UpdateHealthUI();
        ShowDamageText(damage);

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashEffect());
    }

    // This implementation is for the IDamageable interface specifically
    public void TakeDamage(float damage)
    {
        // Redirects to our main TakeDamage logic
        TakeDamage(damage, null);
    }

    // =========================
    // SMEAR
    // =========================
    public void AddSmear(float amount)
    {
        currentSmearIntensity += amount;
        UpdateShaderFloat(berrySmearIntensityName, currentSmearIntensity);
    }

    // =========================
    // SLOW & TICK DAMAGE
    // =========================
    public void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval)
    {
        if (_slowCoroutine != null) StopCoroutine(_slowCoroutine);
        _slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));

        if (_tickDamageCoroutine != null) StopCoroutine(_tickDamageCoroutine);

        // Pass the Enemy itself as the attacker for tick damage 
        // (or you could pass the player if the player started the tick effect)
        _tickDamageCoroutine = StartCoroutine(TickDamageRoutine(tickDmg, tickInterval, duration));
    }

    private IEnumerator TickDamageRoutine(float dmg, float interval, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;

            // We pass null or the player reference here so the killer gets the XP
            TakeDamage(dmg, null);
        }
        _tickDamageCoroutine = null;
    }

    // =========================
    // DEATH
    // =========================
    protected override void Die()
    {
        Debug.Log($"[SAVE] Enemy died: {enemyID}");

        // Reset Shaders
        UpdateShaderFloat(hitIntensityName, 0f);
        UpdateShaderFloat(stunIntensityName, 0f);
        UpdateShaderFloat(berrySmearIntensityName, 0f);

        if (comboSys != null) comboSys.RegisterKill();

        SpawnLoot();

        // base.Die() handles giving XP to the 'lastAttacker' and destroying the object
        base.Die();
    }

    // =========================
    // IMPULSE & AI
    // =========================
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

    // =========================
    // EFFECTS & SHADER
    // =========================
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

    private void UpdateShaderFloat(string name, float value)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(name, value);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    // =========================
    // LOOT
    // =========================
    private void SpawnLoot()
    {
        if (goldPrefab == null) return;

        int amount = Random.Range(minGold, maxGold + 1);
        for (int i = 0; i < amount; i++)
        {
            Instantiate(goldPrefab, transform.position, Quaternion.identity);
        }
    }

    // =========================
    // UI
    // =========================
    public void UpdateHealthUI()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = currentHealth / maxHealth;

        if (healthBarObject != null)
            healthBarObject.SetActive(currentHealth < maxHealth && currentHealth > 0);
    }

    void ShowDamageText(float damage)
    {
        if (damageTextPrefab != null && damage > 0)
        {
            GameObject textObj = Instantiate(damageTextPrefab, transform.position + Vector3.up, Quaternion.identity);
            if (textObj.TryGetComponent<DamageNumber>(out DamageNumber dn))
                dn.Setup(damage);
        }
    }
}