using UnityEngine;

public class EarthHoleDigBehav : MonoBehaviour, IInteractable
{
    private Highlightable highlight;

    private void Awake()
    {
        highlight = GetComponent<Highlightable>();
    }

    public void Interact(InventoryManager playerInventory)
    {
        Debug.Log("Earth hole interacted!");
    }

    public string GetPrompt()
    {
        return "Dig Hole";
    }

    public void OnFocus()
    {
        highlight?.SetHighlighted(true);
    }

    public void OnLoseFocus()
    {
        highlight?.SetHighlighted(false);
    }
}