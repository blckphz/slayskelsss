using UnityEngine;
using System.Collections;

public class treeItemBehav : ItemHealth
{
    [Header("Save State")]
    public bool isCut;
    public float cutTime;

    [Header("Damage")]
    public float axeBonusDamage = 10f;

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

    [Header("VFX")]
    public GameObject leafParticlePrefab;

    [Header("Loot")]
    public GameObject StickPrefab;

    [Header("Stick Spawn Points")]
    public Transform stickSpawn1;
    public Transform stickSpawn2;

    [Range(0f, 1f)]
    public float stickSpawnChancePerDay = 0.2f;

    private int lastCheckedDay = -1;

    public Vector3 damageTextOffset = new Vector3(0, 1.5f, 0);

    private bool isDead = false;
    private Collider2D treeCollider;
    private DayNightCycle timeSystem;

    // ==========================================
    // UNITY
    // ==========================================

    protected override void Awake()
    {
        base.Awake();
        treeCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        timeSystem = FindFirstObjectByType<DayNightCycle>();

        if (isCut)
        {
            SpawnStumpOnly();
            return;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    void Update()
    {
        HandleDailyStickSpawn();
    }

    private void HandleDailyStickSpawn()
    {
        if (timeSystem == null)
            return;

        if (timeSystem.DaysPassed == lastCheckedDay)
            return;

        lastCheckedDay = timeSystem.DaysPassed;
        TrySpawnStick();
    }

    private void TrySpawnStick()
    {
        if (StickPrefab == null)
            return;

        if (Random.value > stickSpawnChancePerDay)
            return;

        Transform spawn = Random.value < 0.5f ? stickSpawn1 : stickSpawn2;

        if (spawn == null)
            return;

        float radius = 0.2f;
        if (Physics2D.OverlapCircle(spawn.position, radius) != null)
            return;

        Instantiate(StickPrefab, spawn.position, Quaternion.identity);
    }

    // ==========================================
    // SAVE / LOAD
    // ==========================================

    public TreeSaveData GetSaveData()
    {
        return new TreeSaveData
        {
            treeID = UniqOverworldItemID,
            isCut = isCut,
            cutTime = cutTime
        };
    }

    public void LoadData(TreeSaveData data)
    {
        if (data == null)
            return;

        isCut = data.isCut;
        cutTime = data.cutTime;

        if (isCut)
            SpawnStumpOnly();
    }

    // ==========================================
    // DAMAGE
    // ==========================================

    public override void TakeDamage(float damage, ToolType usedTool, ItemData toolItem)
    {
        if (isDead)
            return;

        float finalDamage = damage;

        if (usedTool == effectiveTool)
            finalDamage += axeBonusDamage;

        base.TakeDamage(finalDamage, usedTool, toolItem);
    }

    public override void TakeDamage(float damage, ToolType usedTool)
    {
        if (isDead)
            return;

        float finalDamage = damage;

        if (usedTool == effectiveTool)
            finalDamage += axeBonusDamage;

        base.TakeDamage(finalDamage, usedTool);
    }

    public override void TakeDamage(float damage)
    {
        if (isDead)
            return;

        base.TakeDamage(damage);
    }

    // ==========================================
    // DEATH
    // ==========================================

    protected override void Die(ToolType killerTool)
    {
        if (isDead)
            return;

        isDead = true;

        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());

        if (givesXP)
            GrantXP();

        bool correctTool = (killerTool == effectiveTool);

        StartCoroutine(FallSequence(correctTool));
    }

    protected override void Die()
    {
        Die(ToolType.None);
    }

    // ==========================================
    // STUMP
    // ==========================================

    private void SpawnStumpOnly()
    {
        if (bottomPrefab != null)
        {
            GameObject stump = Instantiate(bottomPrefab, transform.position, Quaternion.identity);

            TreeStumpRegrow regrow = stump.GetComponent<TreeStumpRegrow>();

            if (regrow != null)
                regrow.Setup(UniqOverworldItemID, cutTime);
        }

        Destroy(gameObject);
    }

    // ==========================================
    // FALL SEQUENCE
    // ==========================================

    private IEnumerator FallSequence(bool bonusLoot)
    {
        isCut = true;
        cutTime = timeSystem != null ? timeSystem.TotalTime : Time.time;

        if (bottomPrefab != null)
        {
            GameObject stump = Instantiate(bottomPrefab, transform.position, Quaternion.identity);

            TreeStumpRegrow regrow = stump.GetComponent<TreeStumpRegrow>();

            if (regrow != null)
                regrow.Setup(UniqOverworldItemID, cutTime);
        }

        if (leafParticlePrefab != null)
            Instantiate(leafParticlePrefab, transform.position, Quaternion.identity);

        GameObject top = null;

        if (topPrefab != null)
            top = Instantiate(topPrefab, transform.position, Quaternion.identity);

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (treeCollider != null)
            treeCollider.enabled = false;

        yield return new WaitForSeconds(fallDelay);

        Vector2 dir = GetFallDirection();

        if (top != null)
        {
            yield return StartCoroutine(RotateTop(top.transform, dir));

            SpawnTreeLoot(bonusLoot);

            Destroy(top);
        }

        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();

        Destroy(gameObject);
    }

    // ==========================================
    // LOOT
    // ==========================================

    private void SpawnTreeLoot(bool bonusLoot)
    {
        int amount = Random.Range(2, 4);

        if (bonusLoot)
            amount += correctToolUsageBonus;

        for (int i = 0; i < amount; i++)
        {
            GameObject loot = Instantiate(
                lootPrefab,
                transform.position + (Vector3)Random.insideUnitCircle * 0.5f,
                Quaternion.identity
            );

            if (loot.TryGetComponent<LootArc>(out LootArc arc))
                arc.Initialize(transform.position);
        }
    }

    public override void SpawnLoot()
    {
        SpawnTreeLoot(false);
    }

    // ==========================================
    // FALL DIRECTION
    // ==========================================

    private Vector2 GetFallDirection()
    {
        if (player == null)
            return Vector2.right;

        return ((Vector2)transform.position - (Vector2)player.position).normalized;
    }

    // ==========================================
    // ROTATION
    // ==========================================

    private IEnumerator RotateTop(Transform top, Vector2 dir)
    {
        float progress = 0f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion startRot = top.rotation;
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

        SpriteRenderer sr = top.GetComponent<SpriteRenderer>();

        if (sr != null)
            yield return StartCoroutine(FadeOut(sr));
    }

    // ==========================================
    // FADE
    // ==========================================

    private IEnumerator FadeOut(SpriteRenderer sr)
    {
        Color col = sr.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            sr.color = new Color(
                col.r,
                col.g,
                col.b,
                Mathf.Lerp(1f, 0f, t / fadeDuration)
            );

            yield return null;
        }
    }
}