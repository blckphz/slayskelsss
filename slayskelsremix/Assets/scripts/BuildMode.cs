using UnityEngine;
using UnityEngine.InputSystem;

public class BuildMode : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference toggleBuildMode;
    public InputActionReference toggleGrid;

    [Header("Build Mode UI")]
    public CanvasGroup buildModeCanvasGroup;
    public Transform buildModeSprite;

    [Header("Hotbar UI")]
    public CanvasGroup hotbarCanvasGroup;
    public Transform hotbarTransform;

    [Header("Movement")]
    public Vector3 buildModeHiddenOffset = new Vector3(0, -0.2f, 0);
    public Vector3 hotbarHiddenOffset = new Vector3(0, 0.2f, 0);
    public float spriteSpeed = 8f;

    private bool buildMode;
    private bool useGridPlacement = true;

    private Vector3 buildModeShownPos;
    private Vector3 buildModeHiddenPos;

    private Vector3 hotbarShownPos;
    private Vector3 hotbarHiddenPos;

    void Start()
    {
        // Build mode positions
        if (buildModeSprite != null)
        {
            buildModeShownPos = buildModeSprite.localPosition;
            buildModeHiddenPos = buildModeShownPos + buildModeHiddenOffset;
        }

        // Hotbar positions
        if (hotbarTransform != null)
        {
            hotbarShownPos = hotbarTransform.localPosition;
            hotbarHiddenPos = hotbarShownPos + hotbarHiddenOffset;
        }

        // Initial visibility
        if (buildModeCanvasGroup != null)
            buildModeCanvasGroup.alpha = 0f;

        if (hotbarCanvasGroup != null)
            hotbarCanvasGroup.alpha = 1f;
    }

    void OnEnable()
    {
        if (toggleBuildMode != null)
        {
            toggleBuildMode.action.performed += OnToggle;
            toggleBuildMode.action.Enable();
        }

        if (toggleGrid != null)
        {
            toggleGrid.action.performed += OnToggleGrid;
            toggleGrid.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleBuildMode != null)
        {
            toggleBuildMode.action.performed -= OnToggle;
            toggleBuildMode.action.Disable();
        }

        if (toggleGrid != null)
        {
            toggleGrid.action.performed -= OnToggleGrid;
            toggleGrid.action.Disable();
        }
    }

    void Update()
    {
        // Fade build mode IN / OUT
        if (buildModeCanvasGroup != null)
        {
            buildModeCanvasGroup.alpha = Mathf.Lerp(
                buildModeCanvasGroup.alpha,
                buildMode ? 1f : 0f,
                Time.unscaledDeltaTime * spriteSpeed
            );
        }

        // Fade hotbar opposite
        if (hotbarCanvasGroup != null)
        {
            hotbarCanvasGroup.alpha = Mathf.Lerp(
                hotbarCanvasGroup.alpha,
                buildMode ? 0f : 1f,
                Time.unscaledDeltaTime * spriteSpeed
            );
        }

        // Build mode sprite movement
        if (buildModeSprite != null)
        {
            buildModeSprite.localPosition = Vector3.Lerp(
                buildModeSprite.localPosition,
                buildMode ? buildModeShownPos : buildModeHiddenPos,
                Time.unscaledDeltaTime * spriteSpeed
            );
        }

        // Hotbar movement (opposite)
        if (hotbarTransform != null)
        {
            hotbarTransform.localPosition = Vector3.Lerp(
                hotbarTransform.localPosition,
                buildMode ? hotbarHiddenPos : hotbarShownPos,
                Time.unscaledDeltaTime * spriteSpeed
            );
        }
    }

    // =========================================================
    // TOGGLE BUILD MODE
    // =========================================================
    void OnToggle(InputAction.CallbackContext ctx)
    {
        buildMode = !buildMode;

        BuildState.Set(buildMode);

        // TEMP MESSAGE
        if (InteractionUI.Instance != null)
        {
            if (buildMode)
            {
                InteractionUI.Instance.ShowTemporary(
                    "IN BUILD MODE",
                    1.5f
                );
            }
            else
            {
                InteractionUI.Instance.ShowTemporary(
                    "EXIT BUILD MODE",
                    1.5f
                );
            }
        }
    }

    // =========================================================
    // TOGGLE GRID
    // =========================================================
    void OnToggleGrid(InputAction.CallbackContext ctx)
    {
        useGridPlacement = !useGridPlacement;

        BuildState.SetGridPlacement(useGridPlacement);

        // TEMP MESSAGE
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowTemporary(
                useGridPlacement
                    ? "GRID PLACEMENT ON"
                    : "GRID PLACEMENT OFF",
                1f
            );
        }
    }
}