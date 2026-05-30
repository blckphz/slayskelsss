using UnityEngine;
using System.Collections;

public class BushBehav : ItemHealth
{
    [Header("Bush Visuals")]
    public Sprite fullSprite;
    public Sprite harvestedSprite;

    [Header("Bush Settings")]
    public int berriesOnFirstHit = 3;
    public float regrowDays = 1.0f;

    public bool isHarvested = false;
    private float timeAtHarvest = -1f;

    // FIX: restores missing external reference
    public bool IsHarvested => isHarvested;

    private DayNightCycle timeSystem;
    private Coroutine regrowRoutine;

    // ================= UNITY =================

    protected override void Awake()
    {
        base.Awake();
        timeSystem = FindFirstObjectByType<DayNightCycle>();
        EnsureID();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureID();
    }
#endif

    private void EnsureID()
    {
        if (string.IsNullOrEmpty(UniqOverworldItemID))
        {
            UniqOverworldItemID = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    // ================= INVINCIBILITY =================

    protected override bool IsDamageIgnored()
    {
        return isHarvested;
    }

    // ================= DAMAGE =================

    public override void TakeDamage(float damage, ToolType toolType, ItemData toolItem)
    {
        if (!isHarvested)
            HarvestBush();

        base.TakeDamage(damage, toolType, toolItem);
    }

    public override void TakeDamage(float damage, ToolType toolType)
    {
        if (!isHarvested)
            HarvestBush();

        base.TakeDamage(damage, toolType);
    }

    public override void TakeDamage(float damage)
    {
        if (!isHarvested)
            HarvestBush();

        base.TakeDamage(damage);
    }

    // ================= HARVEST =================

    private void HarvestBush()
    {
        isHarvested = true;

        if (timeSystem != null)
            timeAtHarvest = timeSystem.TotalTime;

        if (harvestedSprite != null)
            spriteRenderer.sprite = harvestedSprite;

        DropInitialBerries();
        StartRegrowRoutine();
    }

    private void Regrow()
    {
        isHarvested = false;
        timeAtHarvest = -1f;

        if (fullSprite != null)
        {
            spriteRenderer.sprite = fullSprite;
            health = maxHealth;
        }

        if (regrowRoutine != null)
        {
            StopCoroutine(regrowRoutine);
            regrowRoutine = null;
        }
    }

    // ================= REGROW =================

    private IEnumerator RegrowCheckRoutine()
    {
        while (isHarvested && health > 0)
        {
            if (timeSystem != null &&
                timeSystem.TotalTime >= timeAtHarvest + regrowDays)
            {
                Regrow();
                yield break;
            }

            yield return new WaitForSeconds(5f);
        }
    }

    private void StartRegrowRoutine()
    {
        if (regrowRoutine != null)
            StopCoroutine(regrowRoutine);

        regrowRoutine = StartCoroutine(RegrowCheckRoutine());
    }

    // ================= LOOT =================

    private void DropInitialBerries()
    {
        if (lootPrefab == null) return;

        for (int i = 0; i < berriesOnFirstHit; i++)
        {
            GameObject loot = Instantiate(lootPrefab, transform.position, Quaternion.identity);

            if (loot.TryGetComponent<LootArc>(out LootArc arc))
                arc.Initialize(transform.position);
        }
    }

    // ================= SAVE =================

    public BushSaveData GetSaveData()
    {
        return new BushSaveData
        {
            bushID = UniqOverworldItemID,
            isHarvested = isHarvested,
            health = health,
            timeAtHarvest = timeAtHarvest
        };
    }

    public void LoadData(BushSaveData data)
    {
        if (data == null) return;

        isHarvested = data.isHarvested;
        health = data.health;
        timeAtHarvest = data.timeAtHarvest;

        if (isHarvested && harvestedSprite != null)
            spriteRenderer.sprite = harvestedSprite;
        else if (!isHarvested && fullSprite != null)
            spriteRenderer.sprite = fullSprite;

        if (isHarvested)
            StartRegrowRoutine();
    }
}