using UnityEngine;
using UnityEngine.InputSystem;

public class invUIToggle : MonoBehaviour
{
    public GameObject inventoryPanel;
    [SerializeField] private PlayerAim playerAimScript;
    [SerializeField] private PlayerAttack playerAttackScript; // Reference to your Attack script

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

    private void SetState(bool isOpen)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isOpen);

            // 1. Tell Aim script to center camera
            if (playerAimScript != null)
            {
                playerAimScript.SetInventoryState(isOpen);
            }

            // 2. DISABLE or ENABLE the attack script entirely
            // If inventory is OPEN (true), enabled should be FALSE.
            if (playerAttackScript != null)
            {
                playerAttackScript.enabled = !isOpen;
            }
        }
    }

    public void ForceOpenInventory() => SetState(true);
    public void ForceCloseInventory() => SetState(false);
}