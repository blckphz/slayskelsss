using UnityEngine;
using TMPro; // Required for TextMeshPro

public class CampfireUI : MonoBehaviour
{
    public static CampfireUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject uiBackground;
    [SerializeField] private CampfireSlotUI slotScript;

    [Header("Dynamic Info Text")]
    [SerializeField] private TextMeshProUGUI infoText;

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

    private void Update()
    {
        // DYNAMIC UPDATE: If the panel is open and the fire is burning, 
        // update the text every frame to show the fuel dropping smoothly.
        if (uiPanel != null && uiPanel.activeSelf && currentCampfire != null)
        {
            if (currentCampfire.isBurning)
            {
                UpdateInfoText();
            }
        }
    }

    public void RefreshUI()
    {
        if (uiPanel != null && uiPanel.activeSelf)
        {
            if (slotScript != null) slotScript.UpdateUI();
            UpdateInfoText();
        }
    }

    private void UpdateInfoText()
    {
        if (infoText == null || currentCampfire == null) return;

        // Visual status string
        string status = currentCampfire.isBurning ?
            "<color=#FFA500>BURNING</color>" :
            "<color=#FF4500>EXTINGUISHED</color>";

        // F2 gives you two decimal places (e.g. 5.42) for that high-detail "ticking" feel
        infoText.text = $"<b>CAMPFIRE</b>\n" +
                        $"Status: {status}\n" +
                        $"Fuel: {currentCampfire.fuelAmount:F2} / {currentCampfire.maxFuel}";
    }

    public void OpenCampfire(CampfireBehav campfire)
    {
        if (!campfire) return;

        if (uiPanel != null && uiPanel.activeSelf)
        {
            if (currentCampfire == campfire)
            {
                CloseCampfire();
                return;
            }
        }

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
        }

        RefreshUI();
    }

    public void CloseCampfire()
    {
        if (uiPanel == null || !uiPanel.activeSelf) return;

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

    private void SafeSetActive(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }
}