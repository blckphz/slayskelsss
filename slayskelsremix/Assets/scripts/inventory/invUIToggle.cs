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
        // FIX: If we are already in the target state, stop here.
        // This prevents the "Close" sound from playing if the menu is already closed.
        if (IsInventoryOpen == isOpen) return;

        IsInventoryOpen = isOpen;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isOpen);

            // 🎵 SOUND FX - Only plays if the state actually changed
            PlaySound(isOpen);

            if (playerAimScript != null)
            {
                playerAimScript.SetInventoryState(isOpen);
            }

            if (playerAttackScript != null)
            {
                playerAttackScript.enabled = !isOpen;
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