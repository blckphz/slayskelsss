using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text; // Added for StringBuilder

public class CampfireUI : MonoBehaviour
{
    public static CampfireUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject uiBackground;
    [SerializeField] private CampfireSlotUI slotScript;

    [Header("Waypoints")]
    [SerializeField] private Toggle waypointToggle;

    [Header("Dynamic Info Text")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Inventory Integration")]
    [SerializeField] private invUIToggle inventorySystem;

    public CampfireBehav CurrentCampfire => currentCampfire;
    private CampfireBehav currentCampfire;

    // String Optimization: Cache tags and use StringBuilder to avoid GC pressure
    private StringBuilder infoBuilder = new StringBuilder(128);
    private const string STATUS_BURNING = "<color=#FFA500>BURNING</color>";
    private const string STATUS_OFF = "<color=#FF4500>EXTINGUISHED</color>";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Use cached finding logic
        if (inventorySystem == null)
            inventorySystem = Object.FindFirstObjectByType<invUIToggle>();

        if (waypointToggle != null)
            waypointToggle.onValueChanged.AddListener(OnWaypointToggleChanged);

        SafeSetActive(uiPanel, false);
        SafeSetActive(uiBackground, false);
    }

    private void Update()
    {
        // Only update if panel is visible AND campfire is actually burning/changing
        if (currentCampfire != null && uiPanel.activeSelf)
        {
            // Performance: Only update text if the value has changed significantly 
            // or use a timer (e.g., every 0.1s) to avoid updating 60+ times per second.
            UpdateInfoText();
        }
    }

    private void OnWaypointToggleChanged(bool isOn)
    {
        if (currentCampfire == null) return;

        // Direct call to manager - ensured to only trigger on manual toggle or logic change
        MapPointerManager.Instance.SetLandmark("Campfire", isOn ? currentCampfire.gameObject : null, MapPointerManager.Instance.campfireIcon);
    }

    public void OpenCampfire(CampfireBehav campfire)
    {
        if (!campfire) return;

        // Toggle behavior if clicking the same fire
        if (currentCampfire == campfire && uiPanel.activeSelf)
        {
            CloseCampfire();
            return;
        }

        currentCampfire = campfire;

        // Sync UI State
        if (waypointToggle != null)
        {
            bool isAlreadyTracked = MapPointerManager.Instance.IsTrackingObject("Campfire", currentCampfire.transform);
            waypointToggle.SetIsOnWithoutNotify(isAlreadyTracked);
        }

        SafeSetActive(uiPanel, true);
        SafeSetActive(uiBackground, true);

        if (inventorySystem != null) inventorySystem.ForceOpenInventory();
        if (slotScript) slotScript.campfire = campfire;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (uiPanel == null || !uiPanel.activeSelf) return;

        if (slotScript != null) slotScript.UpdateUI();
        UpdateInfoText();
    }

    private void UpdateInfoText()
    {
        if (infoText == null || currentCampfire == null) return;

        // Optimization: Use StringBuilder to prevent "string + string" garbage allocation
        infoBuilder.Clear();
        infoBuilder.Append("<b>CAMPFIRE</b>\nStatus: ");
        infoBuilder.Append(currentCampfire.isBurning ? STATUS_BURNING : STATUS_OFF);
        infoBuilder.Append("\nFuel: ");
        infoBuilder.Append(currentCampfire.fuelAmount.ToString("F1")); // Reduced precision for cleaner UI
        infoBuilder.Append(" / ");
        infoBuilder.Append(currentCampfire.maxFuel);

        infoText.SetText(infoBuilder);
    }

    public void CloseCampfire()
    {
        if (uiPanel == null || !uiPanel.activeSelf) return;

        SafeSetActive(uiPanel, false);
        SafeSetActive(uiBackground, false);

        if (inventorySystem != null) inventorySystem.ForceCloseInventory();

        currentCampfire = null;
        if (slotScript)
        {
            slotScript.ResetSlot();
            slotScript.campfire = null;
        }
    }

    private void SafeSetActive(GameObject obj, bool state)
    {
        if (obj != null && obj.activeSelf != state) obj.SetActive(state);
    }
}