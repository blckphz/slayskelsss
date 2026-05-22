using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TiltManager : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap tilemap;
    public TileBase soilRuleTile;

    [Header("Camera")]
    public Camera cam;

    [Header("Mode")]
    public bool removeMode;

    private InputAction clickAction;
    private InputAction toggleModeAction;

    void Awake()
    {
        Debug.Log("[TiltManager] Awake called");

        if (cam == null)
        {
            cam = Camera.main;
            Debug.Log("[TiltManager] Camera auto-assigned: " + cam);
        }

        clickAction = new InputAction(
            type: InputActionType.Button,
            binding: "<Pointer>/press"
        );

        toggleModeAction = new InputAction(
            type: InputActionType.Button,
            binding: "<Keyboard>/r"
        );

        Debug.Log("[TiltManager] Input actions created");
    }

    void OnEnable()
    {
        clickAction.Enable();
        toggleModeAction.Enable();

        Debug.Log("[TiltManager] Input enabled");
    }

    void OnDisable()
    {
        clickAction.Disable();
        toggleModeAction.Disable();

        Debug.Log("[TiltManager] Input disabled");
    }

    void Update()
    {
        if (toggleModeAction.WasPressedThisFrame())
        {
            removeMode = !removeMode;
            Debug.Log("[TiltManager] Mode toggled. RemoveMode = " + removeMode);
        }

        if (clickAction.IsPressed())
        {
            Debug.Log("[TiltManager] Click detected");

            if (tilemap == null)
            {
                Debug.LogError("[TiltManager] Tilemap is NULL!");
                return;
            }

            if (soilRuleTile == null)
            {
                Debug.LogError("[TiltManager] SoilRuleTile is NULL!");
                return;
            }

            if (cam == null)
            {
                Debug.LogError("[TiltManager] Camera is NULL!");
                return;
            }

            // ✅ FIXED 2D MOUSE → WORLD CONVERSION
            Vector2 screenPos = Pointer.current.position.ReadValue();

            Vector3 screenPos3 = new Vector3(
                screenPos.x,
                screenPos.y,
                -cam.transform.position.z
            );

            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos3);

            // IMPORTANT for Tilemaps
            worldPos.z = 0f;

            Vector3Int cellPos = tilemap.WorldToCell(worldPos);

            Debug.Log("[TiltManager] Screen: " + screenPos +
                      " World: " + worldPos +
                      " Cell: " + cellPos);

            if (removeMode)
                RemoveSoil(cellPos);
            else
                PlaceSoil(cellPos);
        }
    }

    void PlaceSoil(Vector3Int pos)
    {
        tilemap.SetTile(pos, soilRuleTile);
        Debug.Log("[TiltManager] Placed soil at " + pos);
    }

    void RemoveSoil(Vector3Int pos)
    {
        tilemap.SetTile(pos, null);
        Debug.Log("[TiltManager] Removed soil at " + pos);
    }
}