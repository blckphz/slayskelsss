using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance;

    private PlayerControls controls;

    private Vector2 mousePosition;

    private void Awake()
    {
        Instance = this;
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Gameplay.MousePosition.performed += ctx =>
        {
            mousePosition = ctx.ReadValue<Vector2>();
        };

        controls.Gameplay.Click.performed += OnLeftClick;
        controls.Gameplay.RightClick.performed += OnRightClick;
        controls.Gameplay.Cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // ---------------- INPUT EVENTS ----------------

    private void OnLeftClick(InputAction.CallbackContext ctx)
    {

        // You can route this globally if needed
    }

    private void OnRightClick(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Input] Right Click");
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Input] Cancel (ESC)");
    }

    // ---------------- PUBLIC ACCESS ----------------

    public Vector2 GetMousePosition()
    {
        return mousePosition;
    }

    public bool LeftClickPressed()
    {
        return Mouse.current.leftButton.wasPressedThisFrame;
    }

    public bool RightClickPressed()
    {
        return Mouse.current.rightButton.wasPressedThisFrame;
    }
}