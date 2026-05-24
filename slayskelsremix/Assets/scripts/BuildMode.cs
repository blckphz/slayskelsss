using UnityEngine;
using UnityEngine.InputSystem;

public class BuildMode : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference toggleBuildMode;

    [Header("UI")]
    public HotbarBuildModeUI hotbarUI;

    private bool buildMode;

    private void OnEnable()
    {
        if (toggleBuildMode != null)
        {
            toggleBuildMode.action.performed += OnToggle;
            toggleBuildMode.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleBuildMode != null)
        {
            toggleBuildMode.action.performed -= OnToggle;
            toggleBuildMode.action.Disable();
        }
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        buildMode = !buildMode;

        // Your existing build mode logic
        BuildState.Toggle();

        // Animate hotbar
        if (hotbarUI != null)
            hotbarUI.SetBuildMode(buildMode);
    }
}