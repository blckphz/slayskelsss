using UnityEngine;

public class buildableTurret : MonoBehaviour, IInteractable
{
    private TurretBehaviour turretLogic;
    private BuildIdentity identity;
    private objectHealth healthSystem;

    [Header("Ammo Settings")]
    public int currentAmmo;
    public int maxAmmo = 100;
    public int ammoItemID = 101;
    public int ammoPerStone = 20;

    private bool isInitialized = false;

    void Awake()
    {
        turretLogic = GetComponent<TurretBehaviour>();
        identity = GetComponent<BuildIdentity>();
        healthSystem = GetComponent<objectHealth>();
    }

    void Start()
    {
        if (!isInitialized) InitializeFromBuildItem();
    }

    private void InitializeFromBuildItem()
    {
        if (turretLogic == null || identity == null || identity.item is not turretbuildso data) return;

        if (healthSystem != null) healthSystem.maxHealth = data.turretHealth;

        currentAmmo = maxAmmo;
        turretLogic.Setup(data.turretHealth, 0f, transform);
        turretLogic.SetCombatStats(data.turretDamage, data.fireRate, data.pierceCount);
        isInitialized = true;
    }

    public string GetPrompt()
    {
        if (currentAmmo >= maxAmmo) return "Ammo Full";
        return $"Reload Turret ({currentAmmo}/{maxAmmo})";
    }

    public void Interact(InventoryManager playerInventory)
    {
        ItemData holding = PlayerHotbarManager.Instance.GetSelectedItem();

        if (holding != null && holding.itemID == ammoItemID)
        {
            if (currentAmmo < maxAmmo)
            {
                RefillAmmo(ammoPerStone);
                PlayerHotbarManager.Instance.UseSelectedStack(1);
            }
        }
        else
        {
            Debug.Log("<color=yellow>[Interaction]</color> Wrong item! Need Stones.");
        }
    }

    public void OnFocus() { }
    public void OnLoseFocus() { }

    public bool ConsumeAmmo()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            if (currentAmmo <= 0) turretLogic.SetFiringPermission(false);
            return true;
        }
        return false;
    }

    public void RefillAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        Debug.Log($"<color=green>[Ammo]</color> {gameObject.name} refilled to {currentAmmo}");
        if (currentAmmo > 0) turretLogic.SetFiringPermission(true);
    }
}