public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(InventoryManager playerInventory);
}