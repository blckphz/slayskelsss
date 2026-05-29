using UnityEngine;
using System.Collections;

public class treeItemBehav : ItemHealth
{

    [Header("Save State")]
    public bool isCut;
    public float cutTime;

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

        // LOAD STUMP STATE
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(UniqOverworldItemID))
        {
            UniqOverworldItemID = System.Guid.NewGuid().ToString();
        }
    }
#endif

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
        isCut = data.isCut;
        cutTime = data.cutTime;

        if (isCut)
        {
            SpawnStumpOnly();
        }
    }

    // ==========================================
    // DAMAGE
    // ==========================================

    public override void TakeDamage(float damage, ToolType usedTool, ItemData toolItem)
    {
        if (isDead) return;

        float finalDamage = damage;

        if (usedTool == effectiveTool)
        {
            finalDamage += axeBonusDamage;
        }

        base.TakeDamage(finalDamage, usedTool, toolItem);
    }

    public override void TakeDamage(float damage, ToolType usedTool)
    {
        if (isDead) return;

        float finalDamage = damage;

        if (usedTool == effectiveTool)
        {
            finalDamage += axeBonusDamage;
        }

        base.TakeDamage(finalDamage, usedTool);
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        base.TakeDamage(damage);
    }

    // ==========================================
    // DEATH
    // ==========================================

    protected override void Die(ToolType killerTool)
    {
        if (isDead) return;

        isDead = true;

        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());

        if (givesXP)
            GrantXP();

        bool killedWithCorrectTool =
            (killerTool == effectiveTool);

        StartCoroutine(FallSequence(killedWithCorrectTool));
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
            GameObject stump =
                Instantiate(bottomPrefab,
                transform.position,
                Quaternion.identity);

            TreeStumpRegrow regrow =
                stump.GetComponent<TreeStumpRegrow>();

            if (regrow != null)
            {
                regrow.treeID = UniqOverworldItemID;
            }
        }

        Destroy(gameObject);
    }

    // ==========================================
    // FALL SEQUENCE
    // ==========================================

    private IEnumerator FallSequence(bool bonusLoot)
    {
        // SAVE TREE STATE
        isCut = true;

        cutTime =
            timeSystem != null
            ? timeSystem.TotalTime
            : Time.time;

        // CREATE STUMP
        if (bottomPrefab != null)
        {
            GameObject stump =
                Instantiate(bottomPrefab,
                transform.position,
                Quaternion.identity);

            TreeStumpRegrow regrow =
                stump.GetComponent<TreeStumpRegrow>();

            if (regrow != null)
            {
                regrow.Setup(UniqOverworldItemID, cutTime);
            }
        }

        // LEAF FX
        if (leafParticlePrefab != null)
        {
            Instantiate(
                leafParticlePrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // TREE TOP
        GameObject top = null;

        if (topPrefab != null)
        {
            top = Instantiate(
                topPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // HIDE ORIGINAL TREE
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (treeCollider != null)
            treeCollider.enabled = false;

        yield return new WaitForSeconds(fallDelay);

        // FALL ANIMATION
        Vector2 dir = GetFallDirection();

        if (top != null)
        {
            yield return StartCoroutine(
                RotateTop(top.transform, dir)
            );

            SpawnTreeLoot(bonusLoot);

            Destroy(top);
        }

        // AUTO SAVE
        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.SaveAfterChange();
        }

        Destroy(gameObject);
    }

    // ==========================================
    // LOOT
    // ==========================================

    private void SpawnTreeLoot(bool bonusLoot)
    {

        int amount = Random.Range(2, 4);

        if (bonusLoot)
        {
            amount += correctToolUsageBonus;
        }

        for (int i = 0; i < amount; i++)
        {
            GameObject loot = Instantiate(
                lootPrefab,
                transform.position +
                (Vector3)Random.insideUnitCircle * 0.5f,
                Quaternion.identity
            );

            if (loot.TryGetComponent<LootArc>(out LootArc arc))
            {
                arc.Initialize(transform.position);
            }
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

        return (
            (Vector2)transform.position -
            (Vector2)player.position
        ).normalized;
    }

    // ==========================================
    // ROTATION
    // ==========================================

    private IEnumerator RotateTop(Transform top, Vector2 dir)
    {
        float progress = 0f;

        RulesRotate(
            top,
            dir,
            out Quaternion startRot,
            out Quaternion endRot,
            out Vector3 startPos,
            out Vector3 endPos
        );

        while (progress < 1f)
        {
            progress += Time.deltaTime * fallSpeed;

            top.rotation =
                Quaternion.Lerp(startRot, endRot, progress);

            top.position =
                Vector3.Lerp(startPos, endPos, progress);

            yield return null;
        }

        SpriteRenderer topSR =
            top.GetComponent<SpriteRenderer>();

        if (topSR != null)
        {
            yield return StartCoroutine(FadeOut(topSR));
        }
    }

    private void RulesRotate(
        Transform top,
        Vector2 dir,
        out Quaternion startRot,
        out Quaternion endRot,
        out Vector3 startPos,
        out Vector3 endPos)
    {
        startRot = top.rotation;

        float angle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        endRot =
            Quaternion.Euler(0, 0, angle - 90f);

        startPos = top.position;

        endPos =
            startPos + (Vector3)(dir * fallDistance);
    }

    // ==========================================
    // FADE
    // ==========================================

    private IEnumerator FadeOut(SpriteRenderer sr)
    {
        Color col = sr.color;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            sr.color = new Color(
                col.r,
                col.g,
                col.b,
                Mathf.Lerp(
                    1f,
                    0f,
                    elapsed / fadeDuration
                )
            );

            yield return null;
        }
    }
}