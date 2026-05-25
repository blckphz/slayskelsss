using UnityEngine;
using UnityEngine.InputSystem;

public class BuildMode : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference toggleBuildMode;

    [Header("Hotbar UI")]

    [Header("Sprite (inverse behavior)")]
    public CanvasGroup buildModeSpriteGroup;

    [Header("Sprite movement (optional)")]
    public Transform buildModeSprite;
    public Vector3 spriteHiddenOffset = new Vector3(0, -0.2f, 0);
    public float spriteSpeed = 8f;

    private bool buildMode;

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
    }

    private void OnDisable()
    {
        if (toggleBuildMode != null)
        {
            toggleBuildMode.action.performed -= OnToggle;
            toggleBuildMode.action.Disable();
        }
    }

    private void Update()
    {
        // smooth sprite fade
        if (buildModeSpriteGroup != null)
        {
            buildModeSpriteGroup.alpha = Mathf.Lerp(
                buildModeSpriteGroup.alpha,
                spriteTargetAlpha,
                Time.unscaledDeltaTime * spriteSpeed
            );
        }

        // smooth sprite movement (optional)
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

        // SPRITE (INVERSE behavior)
        spriteTargetAlpha = buildMode ? 1f : 0f;
    }
}