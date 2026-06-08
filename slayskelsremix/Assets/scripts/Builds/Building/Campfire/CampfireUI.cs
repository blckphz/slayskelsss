using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class CampfireUI : MonoBehaviour
{
    public static CampfireUI Instance { get; private set; }

    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject uiBackground;
    [SerializeField] private CampfireSlotUI slotScript;
    [SerializeField] private Toggle waypointToggle;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private invUIToggle inventorySystem;

    public CampfireBehav CurrentCampfire => currentCampfire;
    private CampfireBehav currentCampfire;

    private StringBuilder infoBuilder = new StringBuilder(128);
    private const string STATUS_BURNING = "<color=#FFA500>BURNING</color>";
    private const string STATUS_OFF = "<color=#FF4500>EXTINGUISHED</color>";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (inventorySystem == null) inventorySystem = Object.FindFirstObjectByType<invUIToggle>();
        if (waypointToggle != null) waypointToggle.onValueChanged.AddListener(OnWaypointToggleChanged);

        SafeSetActive(uiPanel, false);
        SafeSetActive(uiBackground, false);
    }

    private void Update()
    {
        if (currentCampfire != null && uiPanel.activeSelf)
            UpdateInfoText();
    }

    private void OnWaypointToggleChanged(bool isOn)
    {
        if (currentCampfire == null) return;
        MapPointerManager.Instance.SetLandmark(
            "Campfire",
            isOn ? currentCampfire.gameObject : null,
            MapPointerManager.Instance.campfireIcon
        );
    }

    public void OpenCampfire(CampfireBehav campfire)
    {
        if (!campfire) return;

        if (currentCampfire == campfire && uiPanel.activeSelf)
        {
            CloseCampfire();
            return;
        }

        currentCampfire = campfire;

        if (waypointToggle != null)
        {
            bool isTracked = MapPointerManager.Instance.IsTrackingObject(
                "Campfire",
                currentCampfire.transform
            );
            waypointToggle.SetIsOnWithoutNotify(isTracked);
        }

        SafeSetActive(uiPanel, true);
        SafeSetActive(uiBackground, true);

        if (inventorySystem != null)
            inventorySystem.ForceOpenInventory();

        if (slotScript)
            slotScript.campfire = campfire;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (uiPanel == null || !uiPanel.activeSelf) return;

        if (slotScript != null)
            slotScript.UpdateUI();

        UpdateInfoText();
    }

    private void UpdateInfoText()
    {
        if (infoText == null || currentCampfire == null) return;

        infoBuilder.Clear();
        infoBuilder.Append("<b>CAMPFIRE</b>\nStatus: ");
        infoBuilder.Append(currentCampfire.isBurning ? STATUS_BURNING : STATUS_OFF);
        infoBuilder.Append("\nFuel: ");
        infoBuilder.Append(currentCampfire.fuelAmount.ToString("F1"));
        infoBuilder.Append(" / ");
        infoBuilder.Append(currentCampfire.maxFuel);

        infoText.SetText(infoBuilder);
    }

    public void CloseCampfire()
    {
        if (uiPanel == null || !uiPanel.activeSelf) return;

        SafeSetActive(uiPanel, false);
        SafeSetActive(uiBackground, false);

        if (inventorySystem != null)
            inventorySystem.ForceCloseInventory();

        currentCampfire = null;

        if (slotScript)
        {
            slotScript.ResetSlot();
            slotScript.campfire = null;
        }
    }

    private void SafeSetActive(GameObject obj, bool state)
    {
        if (obj != null && obj.activeSelf != state)
            obj.SetActive(state);
    }
}