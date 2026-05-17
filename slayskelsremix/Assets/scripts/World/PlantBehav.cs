using UnityEngine;

public class PlantBehav : MonoBehaviour, ISaveableBuilding
{
    [Header("Seed Data")]
    public SeedSO seedData;

    [Header("Growth Settings")]
    public float growTimePerStage = 60f;
    public int maxGrowthStage = 3;

    [Header("Live Stats (Saved)")]
    public int growthStage = 0;
    public float growthProgress = 0f;
    public int currentDurability = 100; // Optional: internal health if plants can be destroyed

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    private void Update()
    {
        // Stop growing if fully grown
        if (growthStage >= maxGrowthStage)
            return;

        // Grow independently per plant
        growthProgress += Time.deltaTime;

        // Advance growth stage
        if (growthProgress >= growTimePerStage)
        {
            growthProgress -= growTimePerStage;
            growthStage++;

            UpdateVisuals();

            // Auto-save when plant changes
            BuildingSaveManager.Instance?.SaveAfterChange();
        }
    }

    // ==========================================
    // SAVE SYSTEM (INTERFACE IMPLEMENTATION)
    // ==========================================

    public int GetItemID()
    {
        return seedData != null ? seedData.itemID : -1;
    }

    public void GetSaveData(out int ammo, out float progress, out int durability)
    {
        // Save growth stage into the "ammo" slot
        ammo = growthStage;

        // Save partial growth progress
        progress = growthProgress;

        // Satisfy interface signature: Use currentDurability or pass a 0 fallback
        durability = currentDurability;
    }

    public void LoadSaveData(int savedStage, float savedProgress, int savedDurability)
    {
        growthStage = savedStage;
        growthProgress = savedProgress;
        currentDurability = savedDurability;

        UpdateVisuals();
    }

    // ==========================================
    // VISUALS
    // ==========================================

    private void UpdateVisuals()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (seedData != null &&
            seedData.growthSprites != null &&
            growthStage < seedData.growthSprites.Length)
        {
            sr.sprite = seedData.growthSprites[growthStage];
        }
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDestroy()
    {
        // Only unregister.
        // DO NOT save here, or quitting the game can wipe your save file.
        BuildingSaveManager.Instance?.UnregisterBuilding(gameObject);
    }
}