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
    private bool layerTogglePressed;
    private bool cancelBuildPressed;
    private bool sleepPressed;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

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


        controls.Gameplay.LayerTogglePressed.performed += ctx =>
        {
            layerTogglePressed = true;
        };


        controls.Gameplay.Cancelbuild.performed += ctx =>
        {
            cancelBuildPressed = true;
        };


        // SLEEP INPUT
        controls.Gameplay.Sleep.performed += ctx =>
        {
            sleepPressed = true;
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
        leftClickPressed = false;
        rightClickPressed = false;
        buildModePressed = false;
        layerTogglePressed = false;
        cancelBuildPressed = false;
        sleepPressed = false;
    }



    private void OnDisable()
    {
        controls.Disable();
    }



    // -------- GETTERS --------


    public Vector2 GetMousePosition()
    {
        return mousePosition;
    }


    public bool LeftClickPressed()
    {
        return leftClickPressed;
    }


    public bool RightClickPressed()
    {
        return rightClickPressed;
    }


    public bool BuildModePressed()
    {
        return buildModePressed;
    }


    public bool LayerTogglePressed()
    {
        return layerTogglePressed;
    }


    public bool CancelBuildPressed()
    {
        return cancelBuildPressed;
    }


    public bool SleepPressed()
    {
        return sleepPressed;
    }



    public bool RightClickHeld()
    {
        return controls != null &&
               controls.Gameplay.RightClick.IsPressed();
    }
}