using UnityEngine;

public class EarthHoleDigBehav : MonoBehaviour, IInteractable
{
    private bool isPlanted;


    public void Interact(InventoryManager playerInventory)
    {
        if (isPlanted) return;

        ItemData item = PlayerHotbarManager.Instance.GetSelectedItem();
        if (item == null) return;

        bool success = item.Use(playerInventory.transform, transform, gameObject);

        if (success)
        {
            PlayerHotbarManager.Instance.UseSelectedStack(item.consumeAmount);
        }
    }

    public bool PlantSeed(GameObject plantPrefab)
    {
        if (plantPrefab == null) return false;

        GameObject newPlant = Instantiate(plantPrefab, transform.position, Quaternion.identity);

        if (newPlant.TryGetComponent(out PlantBehav plant))
        {
            plant.OnPlanted();
        }

        BuildingSaveManager.Instance.RegisterBuilding(newPlant);
        BuildingSaveManager.Instance.SaveNow();

        BuildingSaveManager.Instance.UnregisterBuilding(gameObject);
        Destroy(gameObject);

        isPlanted = true;
        return true;
    }

    public string GetPrompt() => isPlanted ? "" : "Plant Seed";
    public void OnFocus() { }
    public void OnLoseFocus() { }
}