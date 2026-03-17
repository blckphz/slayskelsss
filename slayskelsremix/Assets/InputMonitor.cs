using UnityEngine;
using UnityEngine.InputSystem;

public class InputMonitor : MonoBehaviour
{
    public InputActionReference fireAction;

    void Update()
    {
        if (fireAction == null) return;

        var action = fireAction.action;

        // Check 1: Is the action even enabled?
        if (!action.enabled)
        {
            Debug.LogWarning("<color=red>INPUT ACTION NOT ENABLED!</color>");
            action.Enable(); // Force enable it
        }

        // Check 2: Raw State
        if (action.IsPressed())
        {
            Debug.Log("<color=green>INPUT: Button is being HELD.</color>");
        }

        // Check 3: Frame-perfect triggers
        if (action.WasPressedThisFrame()) Debug.Log("<color=yellow>INPUT: Button PRESSED this frame.</color>");
        if (action.WasReleasedThisFrame()) Debug.Log("<color=white>INPUT: Button RELEASED this frame.</color>");
    }
}