using UnityEngine;
using System.Collections;

public class treeItemBehav : ItemHealth, IDamageable
{
    [Header("Tree Identity")]
    public string treeID;

    [Header("Health Settings")]
    public float health = 10f; // Ensure this matches your ItemHealth logic

    [Header("Loot")]
    public GameObject woodPrefab;

    [Header("Tree Parts")]
    public GameObject topPrefab;
    public GameObject bottomPrefab;

    [Header("Player Reference")]
    public Transform player;

    [Header("Fall Settings")]
    public float fallSpeed = 2f;
    public float fallDistance = 0.5f;
    public float fallDelay = 0.05f;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("VFX & UI")]
    public GameObject leafParticlePrefab;
    public GameObject damageTextPrefab;
    public Vector3 damageTextOffset = new Vector3(0, 1.5f, 0);

    [Header("Flash Settings")]
    public string hitIntensityName = "_Intensity";
    public float flashDuration = 0.2f;

    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D treeCollider;
    private MaterialPropertyBlock propertyBlock;
    private Coroutine _flashCoroutine;
    private DayNightCycle timeSystem;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        treeCollider = GetComponent<Collider2D>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        timeSystem = FindObjectOfType<DayNightCycle>();

        // CHECK IF ALREADY CUT ON LOAD
        if (PlayerPrefs.GetInt("Tree_" + treeID + "_isCut", 0) == 1)
        {
            SpawnStumpOnly();
            return;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        health -= damage;

        if (damageTextPrefab != null && damage > 0)
        {
            GameObject dmgObj = Instantiate(damageTextPrefab, transform.position + damageTextOffset, Quaternion.identity);
            if (dmgObj.TryGetComponent<DamageNumber>(out DamageNumber dn)) dn.Setup(damage);
        }

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashEffect());

        if (health <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(FallSequence());
    }

    private void SpawnStumpOnly()
    {
        // Used when loading the game and finding a tree that should be a stump
        if (bottomPrefab != null)
        {
            GameObject stump = Instantiate(bottomPrefab, transform.position, Quaternion.identity);
            TreeStumpRegrow regrow = stump.GetComponent<TreeStumpRegrow>();
            if (regrow != null) regrow.treeID = treeID;
        }
        Destroy(gameObject);
    }

    private IEnumerator FallSequence()
    {
        if (bottomPrefab != null)
        {
            GameObject stump = Instantiate(bottomPrefab, transform.position, Quaternion.identity);
            TreeStumpRegrow regrow = stump.GetComponent<TreeStumpRegrow>();
            if (regrow != null)
            {
                regrow.Setup(treeID, timeSystem.TotalTime);
            }
        }

        if (leafParticlePrefab != null)
            Instantiate(leafParticlePrefab, transform.position, Quaternion.identity);

        GameObject top = null;
        if (topPrefab != null)
            top = Instantiate(topPrefab, transform.position, Quaternion.identity);

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (treeCollider != null) treeCollider.enabled = false;

        yield return new WaitForSeconds(fallDelay);

        Vector2 dir = GetFallDirection();
        if (top != null)
        {
            yield return StartCoroutine(RotateTop(top.transform, dir));
            SpawnLoot();
            Destroy(top);
        }

        Destroy(gameObject);
    }

    // --- Helper Methods (Flash, Loot, Rotation) ---

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

    private void UpdateShaderFloat(string name, float value)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(name, value);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private void SpawnLoot()
    {
        if (woodPrefab != null)
        {
            int amount = Random.Range(2, 4);
            for (int i = 0; i < amount; i++)
                Instantiate(woodPrefab, transform.position + (Vector3)Random.insideUnitCircle * 0.5f, Quaternion.identity);
        }
    }

    private Vector2 GetFallDirection()
    {
        if (player == null) return Vector2.right;
        return ((Vector2)transform.position - (Vector2)player.position).normalized;
    }

    private IEnumerator RotateTop(Transform top, Vector2 dir)
    {
        float progress = 0f;
        Quaternion startRot = top.rotation;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion endRot = Quaternion.Euler(0, 0, angle - 90f);
        Vector3 startPos = top.position;
        Vector3 endPos = startPos + (Vector3)(dir * fallDistance);

        while (progress < 1f)
        {
            progress += Time.deltaTime * fallSpeed;
            top.rotation = Quaternion.Lerp(startRot, endRot, progress);
            top.position = Vector3.Lerp(startPos, endPos, progress);
            yield return null;
        }

        SpriteRenderer topSR = top.GetComponent<SpriteRenderer>();
        if (topSR != null) yield return StartCoroutine(FadeOut(topSR));
    }

    private IEnumerator FadeOut(SpriteRenderer sr)
    {
        Color col = sr.color;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            sr.color = new Color(col.r, col.g, col.b, Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
    }
}