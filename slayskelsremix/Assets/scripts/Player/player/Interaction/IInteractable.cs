public interface IInteractable
{
    void Interact(InventoryManager playerInventory);
    string GetPrompt();
    void OnFocus();
    void OnLoseFocus();
}