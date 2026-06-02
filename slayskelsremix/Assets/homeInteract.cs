using UnityEngine;
using UnityEngine.InputSystem;

public class homeInteract : MonoBehaviour
{
    private bool playerInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        InteractionUI.Instance.Show("Press E to save game");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        InteractionUI.Instance.Hide();
    }

    private void Update()
    {
        if (!playerInside) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (BuildingSaveManager.Instance != null)
            {
                BuildingSaveManager.Instance.SaveNow();
            }

            InteractionUI.Instance.Show("Game Saved!");

            // re-show prompt after short delay feel
            Invoke(nameof(ShowPromptAgain), 1.2f);
        }
    }

    private void ShowPromptAgain()
    {
        if (playerInside)
            InteractionUI.Instance.Show("Press E to save game");
    }
}