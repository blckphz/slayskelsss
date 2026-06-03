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

    [Header("Bush Audio")]
    public AudioClip harvestSFX;


    public bool isHarvested = false;
    private float timeAtHarvest = -1f;

    private DayNightCycle timeSystem;
    private Coroutine regrowRoutine;

    protected override void Awake()
    {
        base.Awake();
        timeSystem = FindFirstObjectByType<DayNightCycle>();
    }

    // ❌ IMPORTANT FIX:
    // Do NOT block damage pipeline here, or you lose VFX + damage numbers.
    protected override bool IsDamageIgnored()
    {
        return false;
    }

    // =========================
    // DAMAGE ENTRY POINTS
    // =========================

    public override void TakeDamage(int damage, ToolType toolType, ItemData toolItem)
    {
        HandleHit();
        base.TakeDamage(damage, toolType, toolItem);
        CheckForDeath();
    }

    public override void TakeDamage(int damage, ToolType toolType)
    {
        HandleHit();
        base.TakeDamage(damage, toolType);
        CheckForDeath();
    }

    public override void TakeDamage(int damage)
    {
        HandleHit();
        base.TakeDamage(damage);
        CheckForDeath();
    }

    // =========================
    // HARVEST LOGIC
    // =========================

    private void HandleHit()
    {
        if (!isHarvested)
            HarvestBush();
    }

    private void HarvestBush()
    {
        isHarvested = true;

        if (timeSystem != null)
            timeAtHarvest = timeSystem.TotalTime;

        if (harvestedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = harvestedSprite;

        if (harvestSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(harvestSFX);

        DropInitialBerries();
        StartRegrowRoutine();
    }

    private void CheckForDeath()
    {
        if (health <= 0)
        {
            if (deathSFX != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(deathSFX);
        }
    }

    // =========================
    // REGROW SYSTEM
    // =========================

    private void Regrow()
    {
        isHarvested = false;
        timeAtHarvest = -1f;

        if (fullSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = fullSprite;

        health = maxHealth;
    }

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

    // =========================
    // LOOT
    // =========================

    private void DropInitialBerries()
    {
        if (lootPrefab == null) return;

        for (int i = 0; i < berriesOnFirstHit; i++)
        {
            GameObject loot = Instantiate(
                lootPrefab,
                transform.position,
                Quaternion.identity
            );

            if (loot.TryGetComponent(out LootArc arc))
            {
                arc.Initialize(transform.position);
            }
        }
    }

    // =========================
    // SAVE / LOAD
    // =========================

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

        isHarvested = data.isHarvested;
        health = data.health;
        timeAtHarvest = data.timeAtHarvest;
        transform.position = data.position;

        if (spriteRenderer != null)
            spriteRenderer.sprite =
                isHarvested ? harvestedSprite : fullSprite;

        if (isHarvested)
            StartRegrowRoutine();
    }
}