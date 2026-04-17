using UnityEngine;

public class CampfireUI : MonoBehaviour
{
    public static CampfireUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject uiBackground;
    [SerializeField] private CampfireSlotUI slotScript;

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
            Debug.Log("[CampfireUI] OnDestroy - Instance cleared");
        }
    }

    // ---------------------------------------------------
    // OPEN
    // ---------------------------------------------------
    public void OpenCampfire(CampfireBehav campfire)
    {
        Debug.Log("[CampfireUI] OpenCampfire called");

        if (!ValidateReferences()) return;

        if (!campfire)
        {
            return;
        }

        currentCampfire = campfire;

        SafeSetActive(uiPanel, true, "uiPanel (Open)");
        SafeSetActive(uiBackground, true, "uiBackground (Open)");

        if (slotScript)
        {
            slotScript.campfire = campfire;

            slotScript.UpdateUI();
        }
        else
        {
            Debug.LogWarning("[CampfireUI] slotScript missing!");
        }

    }

    // ---------------------------------------------------
    // CLOSE
    // ---------------------------------------------------
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

    // ---------------------------------------------------
    // OUT OF RANGE
    // ---------------------------------------------------
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

    // ---------------------------------------------------
    // VALIDATION
    // ---------------------------------------------------
    private bool ValidateReferences()
    {
        if (!this)
        {
            Debug.LogWarning("[CampfireUI] Object is destroyed");
            return false;
        }

        if (!uiPanel)
        {
            Debug.LogError("[CampfireUI] uiPanel is NULL or DESTROYED");
            return false;
        }

        if (!slotScript)
        {
            Debug.LogError("[CampfireUI] slotScript is NULL or DESTROYED");
            return false;
        }

        return true;
    }

    // ---------------------------------------------------
    // SAFE SET ACTIVE
    // ---------------------------------------------------
    private void SafeSetActive(GameObject obj, bool state, string context)
    {
        if (!obj)
        {
            Debug.LogWarning($"[CampfireUI] Tried SetActive on NULL: {context}");
            return;
        }

        obj.SetActive(state);
        Debug.Log($"[CampfireUI] {context} -> SetActive({state})");
    }
}