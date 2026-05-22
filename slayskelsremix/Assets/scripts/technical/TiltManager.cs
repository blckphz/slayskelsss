using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TiltManager : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase soilTile;

    public Camera cam;

    public bool removeMode;

    public BuildingSaveManager saveManager;

    private InputAction clickAction;
    private InputAction toggleAction;

    void Awake()
    {
        if (cam == null) cam = Camera.main;

        clickAction = new InputAction(type: InputActionType.Button, binding: "<Pointer>/press");
        toggleAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/r");
    }

    void OnEnable()
    {
        clickAction.Enable();
        toggleAction.Enable();
    }

    void OnDisable()
    {
        clickAction.Disable();
        toggleAction.Disable();
    }

    void Update()
    {
        if (toggleAction.WasPressedThisFrame())
            removeMode = !removeMode;

        if (!clickAction.IsPressed()) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();

        Vector3 screenPos3 = new Vector3(
            screenPos.x,
            screenPos.y,
            -cam.transform.position.z
        );

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos3);
        worldPos.z = 0f;

        Vector3Int cell = tilemap.WorldToCell(worldPos);

        if (removeMode)
            Remove(cell);
        else
            Place(cell);
    }

    void Place(Vector3Int pos)
    {
        tilemap.SetTile(pos, soilTile);

    }

    void Remove(Vector3Int pos)
    {
        tilemap.SetTile(pos, null);

    }
}