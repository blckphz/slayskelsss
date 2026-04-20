using UnityEngine;
using UnityEngine.InputSystem;

public class invUIToggle : MonoBehaviour
{
    public GameObject inventoryPanel;
    [SerializeField] private PlayerAim playerAimScript;

    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            SetState(!inventoryPanel.activeSelf);
        }
    }

    public void ForceOpenInventory()
    {
        SetState(true);
    }

    public void ForceCloseInventory()
    {
        SetState(false);
    }

    private void SetState(bool isOpen)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isOpen);

            // Sync with PlayerAim to move camera to player or back to cursor
            if (playerAimScript != null)
            {
                playerAimScript.SetInventoryState(isOpen);
            }
        }
    }
}