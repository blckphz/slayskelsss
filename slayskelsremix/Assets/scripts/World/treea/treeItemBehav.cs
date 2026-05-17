using UnityEngine;
using System.Collections;

public class treeItemBehav : ItemHealth
{
    [Header("Tree Identity")]
    public string treeID;

    [Header("Tool Requirement Settings")]
    public ToolType effectiveTool = ToolType.Axe;
    public float axeBonusDamage = 10f;

    [Header("Loot override")]
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
    public Vector3 damageTextOffset = new Vector3(0, 1.5f, 0);

    private bool isDead = false;
    private Collider2D treeCollider;
    private DayNightCycle timeSystem;

    protected override void Awake()
    {
        base.Awake();
        treeCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        timeSystem = FindObjectOfType<DayNightCycle>();

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

    // 🟢 OVERRIDE: intercepts calculation to inject the extra tool damage modifier
    public override void TakeDamage(float damage, ToolType usedTool)
    {
        if (isDead) return;

        float finalDamage = damage;

        if (usedTool == effectiveTool)
        {
            finalDamage += axeBonusDamage;
        }

        base.TakeDamage(finalDamage);
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        base.TakeDamage(damage);
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());
        StartCoroutine(FallSequence());
    }

    private void SpawnStumpOnly()
    {
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

    public override void SpawnLoot()
    {
        if (woodPrefab != null)
        {
            int amount = Random.Range(2, 4);
            for (int i = 0; i < amount; i++)
            {
                GameObject loot = Instantiate(woodPrefab, transform.position + (Vector3)Random.insideUnitCircle * 0.5f, Quaternion.identity);

                if (loot.TryGetComponent<LootArc>(out LootArc arc))
                {
                    arc.Initialize(transform.position);
                }
            }
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
            elapsed += Time.deltaTime; // Fixed your original script line here which was tracking 'allowed'
            sr.color = new Color(col.r, col.g, col.b, Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
    }
}