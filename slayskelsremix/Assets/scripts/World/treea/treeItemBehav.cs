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

    // NEW OVERLOAD: Intercepts calculation to inject tool damage, checking dynamic item asset values
    public override void TakeDamage(float damage, ToolType usedTool, ItemData toolItem)
    {
        if (isDead) return;

        float finalDamage = damage;

        if (usedTool == effectiveTool)
        {
            finalDamage += axeBonusDamage;
        }

        // Forwards final calculations, tool identifiers, AND item asset properties to base context
        base.TakeDamage(finalDamage, usedTool, toolItem);
    }

    // Intercepts calculation to inject extra tool damage without specific item asset context
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

    // Overridden setup processing death conditions with tool checks
    protected override void Die(ToolType killerTool)
    {
        if (isDead) return;
        isDead = true;

        NPCGlobalEvents.NotifyDestroyed(gameObject.GetInstanceID());

        if (givesXP) GrantXP();

        // Check if the finishing tool used matches the required harvest tool
        bool killedWithCorrectTool = (killerTool == effectiveTool);

        StartCoroutine(FallSequence(killedWithCorrectTool));
    }

    // Fallback requirement for base implementation rules
    protected override void Die()
    {
        Die(ToolType.None);
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

    private IEnumerator FallSequence(bool bonusLoot)
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

            // Runs custom specialized wood production calculation
            SpawnTreeLoot(bonusLoot);
            Destroy(top);
        }

        Destroy(gameObject);
    }

    // Dedicated tree resource controller managing tool dynamic additions
    private void SpawnTreeLoot(bool bonusLoot)
    {
        if (woodPrefab != null)
        {
            // Base generation drop rules (Randomly yields 2 or 3 resources)
            int amount = Random.Range(2, 4);

            // Conditional extra item injection step
            if (bonusLoot)
            {
                amount += correctToolUsageBonus;
            }

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

    // Satisfies abstract declaration needs while directing processes correctly
    public override void SpawnLoot()
    {
        SpawnTreeLoot(false);
    }

    private Vector2 GetFallDirection()
    {
        if (player == null) return Vector2.right;
        return ((Vector2)transform.position - (Vector2)player.position).normalized;
    }

    private IEnumerator RotateTop(Transform top, Vector2 dir)
    {
        float progress = 0f;
        RulesRotate(top, dir, out Quaternion startRot, out Quaternion endRot, out Vector3 startPos, out Vector3 endPos);

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

    private void RulesRotate(Transform top, Vector2 dir, out Quaternion startRot, out Quaternion endRot, out Vector3 startPos, out Vector3 endPos)
    {
        startRot = top.rotation;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        endRot = Quaternion.Euler(0, 0, angle - 90f);
        startPos = top.position;
        endPos = startPos + (Vector3)(dir * fallDistance);
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