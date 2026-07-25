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

    [Header("Camera Shake")]
    public float cameraShakeIntensity = 1.5f;
    public float cameraShakeDuration = 0.15f;

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
    private EnemyAI enemyAI;

    private float originalSpeed;
    private Coroutine _flashCoroutine;
    private Coroutine _slowCoroutine;
    private Coroutine _tickDamageCoroutine;

    public bool IsSlowed => _slowCoroutine != null;

    //=========================
    // SAVE ACCESS
    //=========================
    public float GetHealth() => currentHealth;

    public void SetHealth(int value)
    {
        currentHealth = value;
        Debug.Log($"[{name}] SetHealth() -> {currentHealth}/{maxHealth}");
        UpdateHealthUI();
    }

    //=========================
    // AWAKE
    //=========================
    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
        ai = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
        propertyBlock = new MaterialPropertyBlock();

        if (string.IsNullOrEmpty(enemyID))
            enemyID = System.Guid.NewGuid().ToString();
    }

    private void Start()
    {
        if (ai != null)
            originalSpeed = ai.maxSpeed;

        comboSys = Object.FindAnyObjectByType<comboScript>();
        enemyAI = GetComponent<EnemyAI>();

        Debug.Log($"[{name}] Start() Max Health={maxHealth}, Current={currentHealth}");

        UpdateHealthUI();
    }

    //=========================
    // DAMAGE
    //=========================
    public override void TakeDamage(int damage, GameObject attacker = null)
    {
        Debug.Log($"[{name}] BEFORE DAMAGE: {currentHealth}/{maxHealth}");

        base.TakeDamage(damage, attacker);


        UpdateHealthUI();

        ShowDamageText(damage);

        CameraShaker.Instance?.Shake(cameraShakeIntensity, cameraShakeDuration);

        if (enemyAI != null)
            enemyAI.CancelAttack();

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(FlashEffect());
    }

    public void TakeDamage(int damage, ToolType toolType, ItemData toolItem)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(int damage, ToolType toolType)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, null);
    }

    //=========================
    // SMEAR
    //=========================
    public void AddSmear(float amount)
    {
        currentSmearIntensity += amount;
        UpdateShaderFloat(berrySmearIntensityName, currentSmearIntensity);
    }

    //=========================
    // SLOW
    //=========================
    public void ApplySlow(float slowPercent, float duration, int tickDmg, float tickInterval)
    {
        if (_slowCoroutine != null)
            StopCoroutine(_slowCoroutine);

        _slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));

        if (_tickDamageCoroutine != null)
            StopCoroutine(_tickDamageCoroutine);

        _tickDamageCoroutine = StartCoroutine(TickDamageRoutine(tickDmg, tickInterval, duration));
    }

    private IEnumerator TickDamageRoutine(int dmg, float interval, float duration)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(interval);

            elapsed += interval;

            TakeDamage(dmg, null);
        }

        _tickDamageCoroutine = null;
    }

    //=========================
    // DEATH
    //=========================
    protected override void Die()
    {
        Debug.Log($"[{name}] DIED");
        Debug.Log($"EnemyID: {enemyID}");

        UpdateShaderFloat(hitIntensityName, 0f);
        UpdateShaderFloat(stunIntensityName, 0f);
        UpdateShaderFloat(berrySmearIntensityName, 0f);

        if (comboSys != null)
            comboSys.RegisterKill();

        CameraShaker.Instance?.Shake(cameraShakeIntensity * 1.5f, cameraShakeDuration * 2f);

        SpawnLoot();

        base.Die();
    }

    //=========================
    // IMPULSE
    //=========================
    public void ApplyImpulse(Vector2 force)
    {
        if (rb == null)
            return;

        if (ai != null)
            ai.canMove = false;

        rb.AddForce(force, ForceMode2D.Impulse);

        StopCoroutine(nameof(ResetAI));
        StartCoroutine(ResetAI());
    }

    private IEnumerator ResetAI()
    {
        yield return new WaitForSeconds(0.3f);

        if (ai != null)
            ai.canMove = true;
    }

    //=========================
    // EFFECTS
    //=========================
    private IEnumerator FlashEffect()
    {
        float elapsed = 0;

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

            float elapsed = 0;

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

    private void UpdateShaderFloat(string propertyName, float value)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(propertyName, value);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    //=========================
    // LOOT
    //=========================
    private void SpawnLoot()
    {
        if (goldPrefab == null)
            return;

        int amount = Random.Range(minGold, maxGold + 1);

        Debug.Log($"[{name}] Spawning {amount} gold.");

        for (int i = 0; i < amount; i++)
            Instantiate(goldPrefab, transform.position, Quaternion.identity);
    }

    //=========================
    // HEALTH UI
    //=========================
    public void UpdateHealthUI()
    {

        float fill = Mathf.Clamp01((float)currentHealth / maxHealth);


  

        if (healthBarObject == null)
        {
            Debug.LogError($"[{name}] healthBarObject is NULL!");
        }
        else
        {
            bool show = currentHealth < maxHealth && currentHealth > 0;

            healthBarObject.SetActive(show);

            Debug.Log($"HealthBar Active = {show}");
        }
    }

    //=========================
    // DAMAGE NUMBERS
    //=========================
    private void ShowDamageText(float damage)
    {
        if (damageTextPrefab != null && damage > 0)
        {
            GameObject obj = Instantiate(
                damageTextPrefab,
                transform.position + Vector3.up,
                Quaternion.identity);

            if (obj.TryGetComponent(out DamageNumber dn))
                dn.Setup(damage);
        }
    }
}