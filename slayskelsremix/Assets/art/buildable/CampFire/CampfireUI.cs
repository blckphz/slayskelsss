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
        Instance = this;

        uiPanel.SetActive(false);

        if (uiBackground != null)
            uiBackground.SetActive(false);
    }

    // ---------------------------------------------------
    // OPEN
    // ---------------------------------------------------
    public void OpenCampfire(CampfireBehav campfire)
    {
        currentCampfire = campfire;

        uiPanel.SetActive(true);

        if (uiBackground != null)
            uiBackground.SetActive(true);

        slotScript.campfire = campfire;

        slotScript.UpdateUI();

        Debug.Log("[CampfireUI] Opened");
    }

    // ---------------------------------------------------
    // CLOSE (FIXED - NOW HIDES TEXT PROPERLY)
    // ---------------------------------------------------
    public void CloseCampfire()
    {
        uiPanel.SetActive(false);

        if (uiBackground != null)
            uiBackground.SetActive(false);

        currentCampfire = null;

        if (slotScript != null)
        {
            slotScript.ResetSlot();        // 🔥 clears visuals
            slotScript.campfire = null;
        }

        Debug.Log("[CampfireUI] Closed");
    }

    // ---------------------------------------------------
    // OUT OF RANGE SAFE CALL
    // ---------------------------------------------------
    public void HideDueToRangeExit()
    {
        uiPanel.SetActive(false);

        if (uiBackground != null)
            uiBackground.SetActive(false);

        if (slotScript != null)
            slotScript.HideOutsideRange();

        currentCampfire = null;
    }
}