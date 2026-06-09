using UnityEngine;
using System;

public class PlantBehav : ItemHealth, IInteractable
{
    [Header("Identity")]
    public string plantPrefabName;

    [Header("Growth")]
    public Sprite[] growthSprites;
    public float growTimePerStage = 60f;
    public int maxGrowthStage = 3;

    public int growthStage = 0;
    public float growthProgress = 0f;

    [Header("Harvest")]
    public GameObject producePrefab;
    public int produceAmount = 3;
    public float spawnRadius = 0.5f;

    [Header("Harvest Rules")]
    public bool harvestable = false;
    public bool dontDestroyOnHarvest = true;
    public Sprite harvestedSprite;

    [Header("Audio")]
    public AudioClip harvestSfx;
    public AudioSource audioSource;

    private SpriteRenderer sr;
    private bool isHarvestedState = false;

    private void Awake()
    {
        if (string.IsNullOrEmpty(UniqOverworldItemID))
            UniqOverworldItemID = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateVisuals();
    }

    private void Update()
    {
        growthProgress += Time.deltaTime;

        if (growthProgress >= growTimePerStage)
        {
            growthProgress -= growTimePerStage;

            if (growthStage < maxGrowthStage)
            {
                growthStage++;
            }

            if (growthStage >= maxGrowthStage)
            {
                harvestable = true;
                isHarvestedState = false;
            }

            UpdateVisuals();
            BuildingSaveManager.Instance?.SaveAfterChange();
        }
    }

    public void Interact(InventoryManager playerInventory = null)
    {
        if (!harvestable)
            return;

        Harvest();
    }

    private void Harvest()
    {
        // 🎧 SFX
        if (harvestSfx != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(harvestSfx);
        }

        // Spawn produce
        if (producePrefab != null)
        {
            for (int i = 0; i < produceAmount; i++)
            {
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;

                GameObject loot = Instantiate(
                    producePrefab,
                    transform.position + (Vector3)randomOffset,
                    Quaternion.identity
                );

                if (loot.TryGetComponent(out LootArc arc))
                {
                    arc.Initialize(transform.position);
                }
            }
        }

        if (!dontDestroyOnHarvest)
        {
            Destroy(gameObject);
            return;
        }

        harvestable = false;
        isHarvestedState = true;

        growthStage = 0;
        growthProgress = 0f;

        UpdateVisuals();

        BuildingSaveManager.Instance?.SaveAfterChange();
    }

    public string GetPrompt()
    {
        if (harvestable)
            return "Harvest";

        if (isHarvestedState)
            return "Harvested";

        return "Growing...";
    }

    public void OnFocus() { }
    public void OnLoseFocus() { }

    // ================= SAVE =================

    public PlantSaveData GetPlantSaveData()
    {
        return new PlantSaveData
        {
            plantPrefabName = plantPrefabName,
            plantID = UniqOverworldItemID,
            position = transform.position,
            growthStage = growthStage,
            growthProgress = growthProgress,
        };
    }

    public void LoadPlantSaveData(PlantSaveData data)
    {
        plantPrefabName = data.plantPrefabName;
        UniqOverworldItemID = data.plantID;

        transform.position = data.position;

        growthStage = data.growthStage;
        growthProgress = data.growthProgress;

        UpdateVisuals();
    }

    // ================= VISUALS =================

    private void UpdateVisuals()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (growthSprites == null || growthSprites.Length == 0) return;

        if (isHarvestedState && harvestedSprite != null)
        {
            sr.sprite = harvestedSprite;
            return;
        }

        if (growthStage < growthSprites.Length)
        {
            sr.sprite = growthSprites[growthStage];
        }
    }
}