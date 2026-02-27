using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Pathfinding;

public class enemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float health = 100f;
    private float maxHealth;

    [Header("UI References")]
    public GameObject healthBarObject;
    public Image healthBarFill;
    public GameObject damageTextPrefab;

    [Header("Shader Property Names")]
    public string hitIntensityName = "_Intensity";
    public string stunIntensityName = "_StunIntensity";

    [Header("Effect Intensities")]
    public float peakHitIntensity = 100f;
    public float peakStunIntensity = 100f;
    public float peakSlowIntensity = 100f;

    [Header("Effect Durations")]
    public float flashDuration = 0.2f;

    // Internal Variables
    private AIPath ai;
    private float originalSpeed;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private Coroutine _flashCoroutine;
    private Coroutine _slowCoroutine;
    private Coroutine _stunDamageCoroutine;

    // Interface Implementation: Returns true if the slow coroutine is active
    public bool IsSlowed => _slowCoroutine != null;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ai = GetComponent<AIPath>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        maxHealth = health;
        if (ai != null) originalSpeed = ai.maxSpeed;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        UpdateHealthUI();
        ShowDamageText(damage);

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashEffect());

        if (health <= 0) Die();
    }

    private IEnumerator FlashEffect()
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float currentIntensity = Mathf.Lerp(peakHitIntensity, 0f, elapsed / flashDuration);
            UpdateShaderProperty(hitIntensityName, currentIntensity);
            yield return null;
        }
        UpdateShaderProperty(hitIntensityName, 0f);
    }

    public void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval)
    {
        if (_slowCoroutine != null) StopCoroutine(_slowCoroutine);
        _slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration, tickDmg, tickInterval));
    }

    private IEnumerator SlowRoutine(float slowPercent, float duration, float tickDmg, float tickInterval)
    {
        if (ai != null) ai.maxSpeed = originalSpeed - slowPercent;
        UpdateShaderProperty(stunIntensityName, peakSlowIntensity);

        // Start the repeating damage tick
        if (_stunDamageCoroutine != null) StopCoroutine(_stunDamageCoroutine);
        _stunDamageCoroutine = StartCoroutine(StunDamageTick(tickDmg, tickInterval, duration));

        yield return new WaitForSeconds(duration);

        if (ai != null) ai.maxSpeed = originalSpeed;
        UpdateShaderProperty(stunIntensityName, 0f);

        _slowCoroutine = null;
    }

    private IEnumerator StunDamageTick(float dmg, float interval, float totalDuration)
    {
        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;

            health -= dmg;
            health = Mathf.Clamp(health, 0, maxHealth);
            UpdateHealthUI();
            ShowDamageText(dmg);

            if (health <= 0) { Die(); yield break; }
        }
        _stunDamageCoroutine = null;
    }

    private void UpdateShaderProperty(string name, float value)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(name, value);
        spriteRenderer.SetPropertyBlock(propertyBlock);
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

    void Die() => Destroy(gameObject);
}