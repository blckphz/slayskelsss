using UnityEngine;

public class CraftingRecepieReciever : MonoBehaviour, IInteractable
{
    [SerializeField] private CraftingSO recipeToGive;
    [SerializeField] private string promptMessage = "Read Recipe";

    public void Interact(InventoryManager playerInventory)
    {
        if (recipeToGive == null) return;

        // Call the CraftingManager instead!
        bool learned = CraftingManager.Instance.LearnRecipe(recipeToGive);

        if (learned)
        {
            InteractionUI.Instance?.Hide();
            Destroy(gameObject);
        }
    }

    public string GetPrompt() => promptMessage;
    public void OnFocus() { }
    public void OnLoseFocus() { }
}