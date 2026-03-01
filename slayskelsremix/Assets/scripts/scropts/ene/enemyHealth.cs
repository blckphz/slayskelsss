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

    private AIPath ai;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Coroutine _flashCoroutine;

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
        UpdateHealthUI();
    }

    // --- NEW PHYSICS LOGIC ---
    public void ApplyImpulse(Vector2 force)
    {
        if (rb == null) return;

        // 1. Tell A* to stop fighting the physics engine
        if (ai != null) ai.canMove = false;

        // 2. Apply the "Pull" or "Explosion"
        rb.AddForce(force, ForceMode2D.Impulse);

        // 3. Wait a moment then give control back to AI
        StopCoroutine(nameof(ResetAI));
        StartCoroutine(ResetAI());
    }

    private IEnumerator ResetAI()
    {
        yield return new WaitForSeconds(0.3f); // Duration of the "stun"
        if (ai != null) ai.canMove = true;
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

    private void Die()
    {
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

    private IEnumerator FlashEffect()
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float intensity = Mathf.Lerp(100f, 0f, elapsed / flashDuration);
            UpdateShaderProperty("_Intensity", intensity);
            yield return null;
        }
        UpdateShaderProperty("_Intensity", 0f);
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

    public bool IsSlowed => false;
    public void ApplySlow(float sP, float d, float tD, float tI) { }
}