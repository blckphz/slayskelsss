using UnityEngine;
using UnityEngine.InputSystem;

public class BuildMode : MonoBehaviour
{
    public InputActionReference toggleBuildMode;

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
        BuildState.Toggle();
    }
}