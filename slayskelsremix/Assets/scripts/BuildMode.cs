using UnityEngine;
using UnityEngine.InputSystem;

public class BuildMode : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference toggleBuildMode;
    public InputActionReference toggleGrid;

    [Header("Hotbar UI")]
    public CanvasGroup buildModeSpriteGroup;

    [Header("Sprite movement (optional)")]
    public Transform buildModeSprite;
    public Vector3 spriteHiddenOffset = new Vector3(0, -0.2f, 0);
    public float spriteSpeed = 8f;

    private bool buildMode;
    private bool useGridPlacement = true;

    private Vector3 spriteShownPos;
    private Vector3 spriteHiddenPos;
    private float spriteTargetAlpha;

    private void Start()
    {
        if (buildModeSprite != null)
        {
            spriteShownPos = buildModeSprite.localPosition;
            spriteHiddenPos = spriteShownPos + spriteHiddenOffset;
        }

        if (buildModeSpriteGroup != null)
            buildModeSpriteGroup.alpha = 0f;
    }

    private void OnEnable()
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

    private void OnDisable()
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

    private void Update()
    {
        if (buildModeSpriteGroup != null)
        {
            buildModeSpriteGroup.alpha = Mathf.Lerp(
                buildModeSpriteGroup.alpha,
                spriteTargetAlpha,
                Time.unscaledDeltaTime * spriteSpeed
            );
        }

        if (buildModeSprite != null)
        {
            buildModeSprite.localPosition = Vector3.Lerp(
                buildModeSprite.localPosition,
                buildMode ? spriteShownPos : spriteHiddenPos,
                Time.unscaledDeltaTime * spriteSpeed
            );
        }
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        buildMode = !buildMode;
        BuildState.Toggle();
        spriteTargetAlpha = buildMode ? 1f : 0f;

        // --- NEW CLEANUP LOGIC ---
        if (!buildMode && BuildManager.Instance != null)
        {
            // We use Reflection to call Cancel() since it is private in BuildManager,
            // or you can make Cancel() public in BuildManager.
            // RECOMMENDED: Make Cancel() public in BuildManager.cs
            BuildManager.Instance.Invoke("Cancel", 0f);
        }
    }

    private void OnToggleGrid(InputAction.CallbackContext ctx)
    {
        useGridPlacement = !useGridPlacement;
        BuildState.SetGridPlacement(useGridPlacement);
    }
}