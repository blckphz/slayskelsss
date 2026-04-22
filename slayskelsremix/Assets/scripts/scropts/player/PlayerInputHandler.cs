using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance;

    private PlayerControls controls;

    private Vector2 mousePosition;

    private bool leftClickPressed;
    private bool rightClickPressed;
    private bool buildModePressed;
    private bool layerTogglePressed; // New flag

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Gameplay.MousePosition.performed += ctx =>
        {
            mousePosition = ctx.ReadValue<Vector2>();
        };

        controls.Gameplay.Click.performed += ctx =>
        {
            leftClickPressed = true;
        };

        controls.Gameplay.RightClick.performed += ctx =>
        {
            rightClickPressed = true;
        };

        controls.Gameplay.BuildMode.performed += ctx =>
        {
            buildModePressed = true;
        };

        // --- ADDED LAYER TOGGLE LISTENER ---
        // Make sure "LayerToggle" matches the name in your Input Action Asset exactly
        controls.Gameplay.LayerTogglePressed.performed += ctx =>
        {
            layerTogglePressed = true;
        };
    }

    public void ToggleInput(bool enable)
    {
        if (enable)
            controls.Enable();
        else
            controls.Disable();
    }

    private void LateUpdate()
    {
        // reset "pressed this frame" flags
        leftClickPressed = false;
        rightClickPressed = false;
        buildModePressed = false;
        layerTogglePressed = false; // Reset the new flag
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // ---------------- GETTERS ----------------

    public Vector2 GetMousePosition() => mousePosition;
    public bool LeftClickPressed() => leftClickPressed;
    public bool RightClickPressed() => rightClickPressed;
    public bool BuildModePressed() => buildModePressed;

    // --- ADDED THIS METHOD TO FIX YOUR ERROR ---
    public bool LayerTogglePressed() => layerTogglePressed;

    public bool RightClickHeld()
    {
        return controls != null && controls.Gameplay.RightClick.IsPressed();
    }
}