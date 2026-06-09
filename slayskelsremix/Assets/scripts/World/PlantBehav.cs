using UnityEngine;

public class PlantBehav : MonoBehaviour, ISaveableBuilding
{
    public int itemID;

    public Sprite[] growthSprites;
    public float growTimePerStage = 60f;
    public int maxGrowthStage = 3;

    public int growthStage = 0;
    public float growthProgress = 0f;
    public int currentDurability = 100;

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    private void Update()
    {
        if (growthStage >= maxGrowthStage) return;

        growthProgress += Time.deltaTime;

        if (growthProgress >= growTimePerStage)
        {
            growthProgress -= growTimePerStage;
            growthStage++;

            UpdateVisuals();
            BuildingSaveManager.Instance?.SaveAfterChange();
        }
    }

    public int GetItemID() => itemID;

    public void GetSaveData(out int ammo, out float progress, out int durability)
    {
        ammo = growthStage;
        progress = growthProgress;
        durability = currentDurability;
    }

    public void LoadSaveData(int stage, float progress, int durability)
    {
        growthStage = stage;
        growthProgress = progress;
        currentDurability = durability;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (growthSprites != null &&
            growthStage >= 0 &&
            growthStage < growthSprites.Length)
        {
            sr.sprite = growthSprites[growthStage];
        }
    }

    private void OnDestroy()
    {
        BuildingSaveManager.Instance?.UnregisterBuilding(gameObject);
    }
}