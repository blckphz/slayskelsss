using UnityEngine;

public class objectHealth : healthMaster, ISaveableBuilding
{
    [Header("Interaction")]
    public float interactionRange = 3f;
    private Transform player;

    protected override void Awake()
    {
        base.Awake(); // Setup health

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    public bool IsPlayerInRange()
    {
        if (player == null)
            return true;

        return Vector2.Distance(transform.position, player.position) <= interactionRange;
    }

    public void Deconstruct()
    {
        BuildingSaveManager.Instance?.UnregisterBuilding(gameObject);

        BuildIdentity id = GetComponent<BuildIdentity>();
        if (id != null && id.item != null)
        {
            InventoryManager.Instance?.AddItem(id.item, 1);
        }

        Die(); // Reuse death logic for cleanup
    }

    protected override void Die()
    {
        BuildingSaveManager.Instance?.UnregisterBuilding(gameObject);
        BuildingSaveManager.Instance?.SaveNow();
        base.Die(); // Destroys object
    }

    // =====================================================
    // ISaveableBuilding IMPLEMENTATION
    // =====================================================

    public int GetItemID()
    {
        BuildIdentity id = GetComponent<BuildIdentity>();
        return id != null && id.item != null ? id.item.itemID : -1;
    }

    public void GetSaveData(out int ammo, out float progress, out int durability)
    {
        ammo = 0;        // not used for static buildings like tents/walls
        progress = 0f;   // not used here

        // Use health system as durability
        durability = currentHealth; // from healthMaster
    }

    public void LoadSaveData(int ammo, float progress, int durability)
    {
        currentHealth = durability;
    }
}