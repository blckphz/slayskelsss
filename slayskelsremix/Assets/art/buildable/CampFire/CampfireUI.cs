using UnityEngine;

public class CampfireUI : MonoBehaviour
{
    public static CampfireUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject uiBackground;
    [SerializeField] private CampfireSlotUI slotScript;

    // We expose this so CampfireBehav can check: if (CampfireUI.Instance.CurrentCampfire == this)
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

        SafeSetActive(uiPanel, false, "uiPanel (Awake)");
        SafeSetActive(uiBackground, false, "uiBackground (Awake)");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // This is the method called by CampfireBehav whenever fuel ticks down
    public void RefreshUI()
    {
        if (uiPanel.activeSelf && slotScript != null)
        {
            slotScript.UpdateUI();
        }
    }

    public void OpenCampfire(CampfireBehav campfire)
    {
        if (!ValidateReferences() || !campfire) return;

        currentCampfire = campfire;

        SafeSetActive(uiPanel, true, "uiPanel (Open)");
        SafeSetActive(uiBackground, true, "uiBackground (Open)");

        if (slotScript)
        {
            slotScript.campfire = campfire;
            slotScript.UpdateUI();
        }
    }

    public void CloseCampfire()
    {
        SafeSetActive(uiPanel, false, "uiPanel (Close)");
        SafeSetActive(uiBackground, false, "uiBackground (Close)");

        currentCampfire = null;

        if (slotScript)
        {
            slotScript.ResetSlot();
            slotScript.campfire = null;
        }
    }

    public void HideDueToRangeExit()
    {
        SafeSetActive(uiPanel, false, "uiPanel (RangeExit)");
        SafeSetActive(uiBackground, false, "uiBackground (RangeExit)");

        if (slotScript)
        {
            slotScript.HideOutsideRange();
        }

        currentCampfire = null;
    }

    private bool ValidateReferences()
    {
        if (!this || !uiPanel || !slotScript)
        {
            Debug.LogError("[CampfireUI] Missing critical references!");
            return false;
        }
        return true;
    }

    private void SafeSetActive(GameObject obj, bool state, string context)
    {
        if (!obj) return;
        obj.SetActive(state);
    }
}