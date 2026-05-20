using UnityEngine;

public class BushBehav : ItemHealth
{
    [Header("Save Settings")]
    public string bushID;

    [Header("Bush Visuals")]
    public Sprite fullSprite;
    public Sprite harvestedSprite;

    [Header("Bush Settings")]
    public int berriesOnFirstHit = 3;
    public float regrowDays = 1.0f;

    public bool isHarvested = false;
    private float timeAtHarvest = -1f;

    public bool IsHarvested => isHarvested;

    private DayNightCycle timeSystem;

    protected override void Awake()
    {
        base.Awake();
        timeSystem = Object.FindFirstObjectByType<DayNightCycle>();
    }

    void Update()
    {
        if (isHarvested && health > 0)
        {
            CheckForRegrowth();
        }
    }

    private void CheckForRegrowth()
    {
        if (timeSystem == null) return;

        if (timeSystem.TotalTime >= timeAtHarvest + regrowDays)
        {
            Regrow();
        }
    }

    private void Regrow()
    {
        isHarvested = false;
        timeAtHarvest = -1f;

        if (fullSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = fullSprite;
            health = maxHealth;
        }

        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();
    }

    // ✅ FIX: THIS is the missing override that was breaking everything
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

    private void HarvestBush()
    {
        isHarvested = true;

        if (timeSystem != null)
            timeAtHarvest = timeSystem.TotalTime;

        if (harvestedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = harvestedSprite;

        DropInitialBerries();

        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();
    }

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

    public void LoadData(BushSaveData data)
    {
        if (data == null) return;

        isHarvested = data.isHarvested;
        health = data.health;
        timeAtHarvest = data.timeAtHarvest;

        if (isHarvested && harvestedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = harvestedSprite;
        else if (!isHarvested && fullSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = fullSprite;

        if (health <= 0)
            gameObject.SetActive(false);
    }

    public BushSaveData GetSaveData()
    {
        return new BushSaveData
        {
            bushID = bushID,
            isHarvested = isHarvested,
            health = health,
            timeAtHarvest = timeAtHarvest
        };
    }
}