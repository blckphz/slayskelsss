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

    // =========================
    // SAVE SYSTEM
    // =========================

    public int GetItemID()
    {
        return seedData.itemID;
    }

    public void GetSaveData(out int ammo, out float progress)
    {
        // Save growth stage
        ammo = growthStage;

        // Save partial growth progress
        progress = growthProgress;
    }

    public void LoadSaveData(int savedStage, float savedProgress)
    {
        growthStage = savedStage;
        growthProgress = savedProgress;

        UpdateVisuals();

    }

    // =========================
    // VISUALS
    // =========================

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

    // =========================
    // CLEANUP
    // =========================

    private void OnDestroy()
    {
        // Only unregister.
        // DO NOT save here, or quitting the game can wipe your save file.
        BuildingSaveManager.Instance?.UnregisterBuilding(gameObject);
    }
}