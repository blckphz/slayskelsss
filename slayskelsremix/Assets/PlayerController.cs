using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public InputActionAsset inputActions;

    private InputAction click;
    private InputAction rightClick;
    private InputAction mousePos;

    private void Awake()
    {
        Instance = this;

        click = inputActions.FindAction("Click");
        rightClick = inputActions.FindAction("RightClick");
        mousePos = inputActions.FindAction("MousePosition");
    }

    private void OnEnable()
    {
        click.Enable();
        rightClick.Enable();
        mousePos.Enable();
    }

    private void OnDisable()
    {
        click.Disable();
        rightClick.Disable();
        mousePos.Disable();
    }

    public bool Clicked()
    {
        return click.WasPressedThisFrame();
    }

    public bool RightClicked()
    {
        return rightClick.WasPressedThisFrame();
    }

    public Vector2 MousePosition()
    {
        return mousePos.ReadValue<Vector2>();
    }
}