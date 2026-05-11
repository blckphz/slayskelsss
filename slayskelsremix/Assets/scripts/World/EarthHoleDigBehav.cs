using UnityEngine;

public class EarthHoleDigBehav : MonoBehaviour, IInteractable
{
    private bool isPlanted;

    public void Interact(InventoryManager playerInventory)
    {
        if (isPlanted) return;

        ItemData item = PlayerHotbarManager.Instance.GetSelectedItem();
        if (item == null) return;

        // Use the item (Seed/Berry)
        bool success = item.Use(playerInventory.transform, transform, gameObject);

        if (success)
        {
            PlayerHotbarManager.Instance.UseSelectedStack(item.consumeAmount);
        }
    }

    public bool PlantSeed(GameObject plantPrefab)
    {
        if (plantPrefab == null) return false;

        // 1. Spawn
        GameObject newPlant = Instantiate(plantPrefab, transform.position, Quaternion.identity);
        newPlant.name = plantPrefab.name;

        // 2. IMPORTANT: Manually trigger registration to be 100% sure
        BuildingSaveManager.Instance.RegisterBuilding(newPlant);

        // 3. Save immediately
        BuildingSaveManager.Instance.SaveNow();

        // 4. Cleanup Hole
        BuildingSaveManager.Instance.UnregisterBuilding(gameObject);
        Destroy(gameObject);

        return true;
    }

    public string GetPrompt() => isPlanted ? "" : "Plant Seed";
    public void OnFocus() { /* Highlight logic */ }
    public void OnLoseFocus() { /* Unhighlight logic */ }
}