using UnityEngine;
using UnityEngine.InputSystem; // Essential: You need this namespace!

public class invUIToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    void Update()
    {
        // Checks if the 'I' key was pressed this frame
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            // Sets the active state to the opposite of what it currently is
            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);
        }
    }
}