using UnityEngine;

public class BushBehav : ItemHealth
{
    [Header("Save Settings")]
    public string bushID;

    [Header("Bush Visuals")]
    public Sprite fullSprite;       // The sprite with berries
    public Sprite harvestedSprite;  // The empty bush sprite

    [Header("Bush Settings")]
    public int berriesOnFirstHit = 3;
    public float regrowDays = 1.0f; // How many days to regrow? (1.0 = 24 hours)

    private bool isHarvested = false;
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
        // Only check for regrowth if it's harvested and the bush isn't "dead"
        if (isHarvested && health > 0)
        {
            CheckForRegrowth();
        }
    }

    private void CheckForRegrowth()
    {
        if (timeSystem == null) return;

        // If the current TotalTime is greater than harvest time + duration, regrow
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
            health = maxHealth; // Reset health when regrowing

        }

        // Optional: Save after regrowth so it stays full on next load
        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();
    }

    public override void TakeDamage(float damage)
    {
        if (!isHarvested)
        {
            HarvestBush();
        }
        base.TakeDamage(damage);
    }

    private void HarvestBush()
    {
        isHarvested = true;

        // Mark the exact time it was harvested from the DayNightCycle
        if (timeSystem != null)
            timeAtHarvest = timeSystem.TotalTime;

        if (harvestedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = harvestedSprite;
        }

        DropInitialBerries();

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.SaveAfterChange();
        }
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

        this.isHarvested = data.isHarvested;
        this.health = data.health;
        this.timeAtHarvest = data.timeAtHarvest;

        if (isHarvested && harvestedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = harvestedSprite;
        else if (!isHarvested && fullSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = fullSprite;

        if (this.health <= 0)
            gameObject.SetActive(false);
    }

    // Update your GetSaveData method in BuildingSaveManager to include timeAtHarvest
    public BushSaveData GetSaveData()
    {
        return new BushSaveData
        {
            bushID = this.bushID,
            isHarvested = this.isHarvested,
            health = this.health,
            timeAtHarvest = this.timeAtHarvest
        };
    }
}