using UnityEngine;

public class ChestInteract : MonoBehaviour
{
    private ChestInventory chest;

    private void Awake()
    {
        chest = GetComponent<ChestInventory>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[ChestInteract] Player ENTERED chest");

        if (ChestUI.Instance == null) return;

        // Close any previously open chest
        ChestInventory current = ChestUI.Instance.GetCurrentChest();
        if (current != null && current != chest)
        {
            current.CloseChest();
        }

        chest.OpenChest();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[ChestInteract] Player EXITED chest");

        if (ChestUI.Instance != null && ChestUI.Instance.GetCurrentChest() == chest)
        {
            chest.CloseChest();
        }
    }
}