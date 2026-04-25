using UnityEngine;

public class PerkManager : MonoBehaviour
{
    public AbilityDatabase abilityDatabase;
    public PerkUIDisplay uiDisplay;

    void Awake()
    {
        if (uiDisplay == null) uiDisplay = Object.FindFirstObjectByType<PerkUIDisplay>();

        if (abilityDatabase == null)
        {
            Debug.LogError("<color=red>[PerkManager]</color> AbilityDatabase is MISSING in Inspector!");
            return;
        }

        InitializeButtons();
    }

    public void InitializeButtons()
    {
        PerkButton[] buttons = GetComponentsInChildren<PerkButton>(true);
        Debug.Log($"<color=cyan>[PerkManager]</color> Handshaking {buttons.Length} buttons.");

        foreach (PerkButton btn in buttons)
        {
            btn.SetupButton(this, uiDisplay);
        }
    }
}