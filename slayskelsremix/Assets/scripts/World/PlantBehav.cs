using UnityEngine;
using System;

public class PlantBehav : ItemHealth, IInteractable
{
    [Header("Identity")]
    public string plantPrefabName;

    [Header("Growth (Time Based)")]
    public Sprite[] growthSprites;
    public int maxGrowthStage = 3;
    public int growthStage = 0;

    public float growTimePerStage = 60f;
    private float growthTimer = 0f;

    [Header("Water Gate System")]
    public int waterLevel = 0;
    public int maxWater = 3;

    public float dryInterval = 1f;
    private float dryTimer = 0f;

    [Header("Harvest")]
    public GameObject producePrefab;
    public int produceAmount = 3;
    public float spawnRadius = 0.5f;

    [Header("Harvest Rules")]
    public bool harvestable = false;
    public bool dontDestroyOnHarvest = true;

    [Header("Harvest Visuals")]
    public Sprite harvestableSprite;
    public Sprite harvestedSprite;

    [Header("Audio")]
    public AudioClip harvestSfx;
    public AudioClip waterSfx;
    public AudioClip plantSfx;   // ✅ NEW PLANT SOUND
    public AudioSource audioSource;

    private SpriteRenderer sr;
    private bool isHarvestedState = false;
    private bool hasPlayedPlantSfx = false;

    // ================= UNITY =================

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

    // 👉 CALL THIS WHEN PLANT IS FIRST CREATED
    public void OnPlanted()
    {
        if (hasPlayedPlantSfx) return;

        hasPlayedPlantSfx = true;

        if (AudioManager.Instance != null && plantSfx != null)
        {
            AudioManager.Instance.PlaySound(plantSfx);
        }
    }

    private void Update()
    {
        if (harvestable) return;

        // 💧 WATER DRAIN
        dryTimer += Time.deltaTime;

        if (dryTimer >= dryInterval)
        {
            dryTimer = 0f;

            if (waterLevel > 0)
                waterLevel--;
        }

        // 🚫 BLOCK GROWTH IF DRY
        if (waterLevel <= 0)
            return;

        // 🌱 TIME-BASED GROWTH
        growthTimer += Time.deltaTime;

        if (growthTimer >= growTimePerStage)
        {
            growthTimer = 0f;
            GrowOneStage();
        }
    }

    // ================= WATER SYSTEM =================

    public void Water(int amount)
    {
        if (harvestable) return;

        int oldWater = waterLevel;

        waterLevel += amount;
        waterLevel = Mathf.Clamp(waterLevel, 0, maxWater);

        if (waterLevel > oldWater)
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (waterSfx != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(waterSfx);
            }
        }
    }

    // ================= GROWTH =================

    private void GrowOneStage()
    {
        if (growthStage < maxGrowthStage)
        {
            growthStage++;
            Debug.Log($"[Plant] Grew to stage {growthStage}");
        }

        if (growthStage >= maxGrowthStage)
        {
            harvestable = true;
            isHarvestedState = false;
        }

        UpdateVisuals();
        BuildingSaveManager.Instance?.SaveAfterChange();
    }

    // ================= INTERACTION =================

    public void Interact(InventoryManager playerInventory = null)
    {
        if (harvestable)
        {
            Harvest();
            return;
        }

        TryWater(playerInventory);
    }

    private void TryWater(InventoryManager inv)
    {
        if (inv == null) return;

        var hotbar = PlayerHotbarManager.Instance;
        if (hotbar == null) return;

        int index = hotbar.selectedIndex;
        if (index < 0 || index >= inv.hotbarData.Count) return;

        var slot = inv.hotbarData[index];
        if (slot?.item == null) return;

        if (slot.item is buckeSO)
        {
            if (slot.currentDurability <= 0) return;

            int waterAmount = 1;

            Water(waterAmount);

            slot.currentDurability -= waterAmount;
            slot.currentDurability = Mathf.Max(0, slot.currentDurability);

            hotbar.RefreshHotbar();
            inv.SaveInventory();

            Debug.Log("[Plant] Watered!");
        }
    }

    // ================= HARVEST =================

    private void Harvest()
    {
        if (harvestSfx != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(harvestSfx);
        }

        if (producePrefab != null)
        {
            for (int i = 0; i < produceAmount; i++)
            {
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;

                Instantiate(
                    producePrefab,
                    transform.position + (Vector3)randomOffset,
                    Quaternion.identity
                );
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
        growthTimer = 0f;
        waterLevel = 0;

        UpdateVisuals();
        BuildingSaveManager.Instance?.SaveAfterChange();
    }

    // ================= UI =================

    public string GetPrompt()
    {
        if (harvestable)
            return "Harvest";

        return "Water";
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
            waterLevel = waterLevel
        };
    }

    public void LoadPlantSaveData(PlantSaveData data)
    {
        plantPrefabName = data.plantPrefabName;
        UniqOverworldItemID = data.plantID;

        transform.position = data.position;

        growthStage = data.growthStage;
        waterLevel = data.waterLevel;

        harvestable = (growthStage >= maxGrowthStage);
        isHarvestedState = false;

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

        if (harvestable && harvestableSprite != null)
        {
            sr.sprite = harvestableSprite;
            return;
        }

        if (growthStage >= 0 && growthStage < growthSprites.Length)
        {
            sr.sprite = growthSprites[growthStage];
        }
    }
}