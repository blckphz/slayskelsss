using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Pathfinding;

public class enemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float health = 100f;
    private float maxHealth;

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

    private AIPath ai;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private comboScript comboSys;

    private float originalSpeed;
    private Coroutine _flashCoroutine;
    private Coroutine _slowCoroutine;
    private Coroutine _tickDamageCoroutine;

    // IMPLEMENTING INTERFACE PROPERTY
    public bool IsSlowed => _slowCoroutine != null;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ai = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        maxHealth = health;
        if (ai != null) originalSpeed = ai.maxSpeed;

        // Find the combo script in the scene automatically
        comboSys = Object.FindAnyObjectByType<comboScript>();

        UpdateHealthUI();
    }

    // --- IDAMAGEABLE METHODS ---


    public void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval)
    {
        if (_slowCoroutine != null) StopCoroutine(_slowCoroutine);
        _slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));

        if (_tickDamageCoroutine != null) StopCoroutine(_tickDamageCoroutine);
        _tickDamageCoroutine = StartCoroutine(TickDamageRoutine(tickDmg, tickInterval, duration));
    }

    // --- PHYSICS & EFFECTS ---

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
            // Reduce speed
            ai.maxSpeed = Mathf.Max(0.1f, originalSpeed - slowAmount);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pulse = 0.85f + Mathf.Sin(Time.time * 10f) * 0.35f;
                UpdateShaderFloat(stunIntensityName, pulse);
                yield return null;
            }
            ai.maxSpeed = originalSpeed;
            UpdateShaderFloat(stunIntensityName, 0f);
        }
        _slowCoroutine = null; // Resetting this makes IsSlowed return false
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

    // --- DEATH AND UI ---

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
        ShowDamageText(damage);

        // REMOVED: comboSys.RegisterHit() - We only want kills now!

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashEffect());

        if (health <= 0) Die();
    }

    private void Die()
    {
        UpdateShaderFloat(hitIntensityName, 0f);
        UpdateShaderFloat(stunIntensityName, 0f);

        // ONLY CALL COMBO SYSTEM HERE
        if (comboSys != null)
        {
            comboSys.RegisterKill();
        }

        SpawnLoot();
        Destroy(gameObject);
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
        if (healthBarFill != null) healthBarFill.fillAmount = health / maxHealth;
        if (healthBarObject != null) healthBarObject.SetActive(health < maxHealth);
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