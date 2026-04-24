using UnityEngine;

public class CampfireUI : MonoBehaviour
{
    public static CampfireUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject uiBackground;
    [SerializeField] private CampfireSlotUI slotScript;

    [Header("Inventory Integration")]
    [SerializeField] private invUIToggle inventorySystem;

    public CampfireBehav CurrentCampfire => currentCampfire;
    private CampfireBehav currentCampfire;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (inventorySystem == null)
            inventorySystem = Object.FindFirstObjectByType<invUIToggle>();

        SafeSetActive(uiPanel, false);
        SafeSetActive(uiBackground, false);
    }

    public void RefreshUI()
    {
        if (uiPanel != null && uiPanel.activeSelf && slotScript != null)
        {
            slotScript.UpdateUI();
        }
    }

    // This is the method called by the 'E' interaction
    public void OpenCampfire(CampfireBehav campfire)
    {
        if (!campfire) return;

        // CHECK: If the panel is ALREADY open...
        if (uiPanel != null && uiPanel.activeSelf)
        {
            // ...and it's the SAME campfire, close everything and stop.
            if (currentCampfire == campfire)
            {
                CloseCampfire();
                return;
            }
            // If it's a DIFFERENT campfire, we just switch the reference (rare case)
        }

        // Otherwise, open it up
        currentCampfire = campfire;

        SafeSetActive(uiPanel, true);
        SafeSetActive(uiBackground, true);

        if (inventorySystem != null)
        {
            inventorySystem.ForceOpenInventory();
        }

        if (slotScript)
        {
            slotScript.campfire = campfire;
            slotScript.UpdateUI();
        }
    }

    public void CloseCampfire()
    {
        SafeSetActive(uiPanel, false);
        SafeSetActive(uiBackground, false);

        if (inventorySystem != null)
        {
            inventorySystem.ForceCloseInventory();
        }

        currentCampfire = null;

        if (slotScript)
        {
            slotScript.ResetSlot();
            slotScript.campfire = null;
        }
    }

    public void HideDueToRangeExit()
    {
        CloseCampfire();
    }

    private bool ValidateReferences()
    {
        return uiPanel != null && slotScript != null;
    }

    private void SafeSetActive(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }
}