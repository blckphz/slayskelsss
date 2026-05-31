using UnityEngine;
using UnityEngine.InputSystem;

public class invUIToggle : MonoBehaviour
{
    public GameObject inventoryPanel;
    [SerializeField] private PlayerAim playerAimScript;
    [SerializeField] private PlayerAttack playerAttackScript;

    [Header("Sound FX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    public static bool IsInventoryOpen { get; private set; }

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
        // prevent re-applying same state
        if (IsInventoryOpen == isOpen) return;

        IsInventoryOpen = isOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isOpen);

        PlaySound(isOpen);

        if (playerAimScript != null)
            playerAimScript.SetInventoryState(isOpen);

        if (playerAttackScript != null)
            playerAttackScript.enabled = !isOpen;

        // ✅ IMPORTANT: cancel drag visuals when closing
        if (!isOpen)
        {
            ForceStopAllSlotDrags();
        }
    }

    private void ForceStopAllSlotDrags()
    {
        // Hotbar slots
        if (PlayerHotbarManager.Instance != null)
        {
            foreach (var slot in PlayerHotbarManager.Instance.hotbarSlots)
            {
                if (slot != null)
                    slot.ForceStopDrag();
            }
        }

        // Inventory grid slots
        if (InvUI.Instance != null && InvUI.Instance.slotMappings != null)
        {
            foreach (var data in InvUI.Instance.slotMappings)
            {
                if (data.slotUI != null)
                    data.slotUI.ForceStopDrag();
            }
        }
    }

    private void PlaySound(bool isOpen)
    {
        if (audioSource == null) return;

        if (isOpen && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        else if (!isOpen && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
    }

    public void ForceOpenInventory() => SetState(true);
    public void ForceCloseInventory() => SetState(false);
}