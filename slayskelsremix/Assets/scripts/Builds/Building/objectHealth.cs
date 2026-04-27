using UnityEngine;

public class objectHealth : healthMaster
{
    [Header("Interaction")]
    public float interactionRange = 3f;
    private Transform player;

    protected override void Awake()
    {
        base.Awake(); // Setup health
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    public bool IsPlayerInRange()
    {
        if (player == null) return true;
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
}