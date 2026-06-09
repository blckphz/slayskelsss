public interface IInteractable
{
    void Interact(InventoryManager playerInventory = null);
    string GetPrompt();
    void OnFocus();
    void OnLoseFocus();
}