using System.Collections;
using UnityEngine;

public class BushBehav : ItemHealth
{
    [Header("Bush Visuals")]
    public Sprite fullSprite;
    public Sprite harvestedSprite;

    [Header("Bush Settings")]
    public int berriesOnFirstHit = 3;
    public float regrowDays = 1f;

    public bool isHarvested = false;
    private float timeAtHarvest = -1f;

    private DayNightCycle timeSystem;
    private Coroutine regrowRoutine;

    protected override void Awake()
    {
        base.Awake();
        timeSystem = FindFirstObjectByType<DayNightCycle>();
    }

    protected override bool IsDamageIgnored() => isHarvested;

    public override void TakeDamage(int damage, ToolType toolType, ItemData toolItem)
    {
        if (!isHarvested) HarvestBush();
        base.TakeDamage(damage, toolType, toolItem);
    }

    public override void TakeDamage(int damage, ToolType toolType)
    {
        if (!isHarvested) HarvestBush();
        base.TakeDamage(damage, toolType);
    }

    public override void TakeDamage(int damage)
    {
        if (!isHarvested) HarvestBush();
        base.TakeDamage(damage);
    }

    private void HarvestBush()
    {
        isHarvested = true;
        if (timeSystem != null) timeAtHarvest = timeSystem.TotalTime;
        if (harvestedSprite != null) spriteRenderer.sprite = harvestedSprite;

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
    }

    private IEnumerator RegrowCheckRoutine()
    {
        while (isHarvested && health > 0)
        {
            if (timeSystem != null && timeSystem.TotalTime >= timeAtHarvest + regrowDays)
            {
                Regrow();
                yield break;
            }
            yield return new WaitForSeconds(5f);
        }
    }

    private void StartRegrowRoutine()
    {
        if (regrowRoutine != null) StopCoroutine(regrowRoutine);
        regrowRoutine = StartCoroutine(RegrowCheckRoutine());
    }

    private void DropInitialBerries()
    {
        if (lootPrefab == null) return;

        for (int i = 0; i < berriesOnFirstHit; i++)
        {
            GameObject loot = Instantiate(lootPrefab, transform.position, Quaternion.identity);

            LootArc arc = loot.GetComponent<LootArc>();
            if (arc != null)
            {
                arc.Initialize(transform.position);
            }
        }
    }

    // ================= PERSISTENCE =================

    public BushSaveData GetSaveData()
    {
        return new BushSaveData
        {
            uniqID = UniqOverworldItemID,
            isHarvested = isHarvested,
            health = health,
            timeAtHarvest = timeAtHarvest,
            position = transform.position
        };
    }

    public void LoadData(BushSaveData data)
    {
        if (data == null) return;

        // Restore state
        isHarvested = data.isHarvested;
        health = data.health;
        timeAtHarvest = data.timeAtHarvest;
        transform.position = data.position;

        // Immediate visual update
        if (spriteRenderer != null)
            spriteRenderer.sprite = isHarvested ? harvestedSprite : fullSprite;

        // Restart regrow logic if the bush is still in a harvested state
        if (isHarvested)
            StartRegrowRoutine();
    }
}