using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TiltManager : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public TileBase soilTile;
    public Camera cam;
    public BuildingSaveManager saveManager;

    [Header("Input")]
    public InputActionReference removeSoil;

    [Header("Settings")]
    public bool removeMode;

    private InputAction clickAction;
    private bool clickHeld;

    private Vector3Int lastCell = new Vector3Int(int.MinValue, int.MinValue, 0);

    private readonly HashSet<Vector3Int> placedTiles = new();
    private PlayerHotbarManager hotbar;
    private float camZ;
    private CompositeCollider2D compositeCollider;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        camZ = -cam.transform.position.z;
        compositeCollider = tilemap.GetComponent<CompositeCollider2D>();

        clickAction = new InputAction(type: InputActionType.Button, binding: "<Pointer>/press");

    }

    void OnEnable()
    {
        clickAction.started += OnClickDown;
        clickAction.canceled += OnClickUp;
        clickAction.Enable();


        if (removeSoil != null)
        {
            removeSoil.action.performed += ToggleRemoveMode;
            removeSoil.action.Enable();
        }

        hotbar = PlayerHotbarManager.Instance;
    }

    void OnDisable()
    {
        clickAction.started -= OnClickDown;
        clickAction.canceled -= OnClickUp;
        clickAction.Disable();


        if (removeSoil != null)
        {
            removeSoil.action.performed -= ToggleRemoveMode;
            removeSoil.action.Disable();
        }
    }

    void OnClickDown(InputAction.CallbackContext ctx)
    {
        clickHeld = true;
    }

    void OnClickUp(InputAction.CallbackContext ctx)
    {
        clickHeld = false;
        lastCell = new Vector3Int(int.MinValue, int.MinValue, 0);

    }

    void Update()
    {
        if (!BuildState.IsBuildMode || !IsShovelEquipped())
        {
            clickHeld = false;
            return;
        }

        // 🔒 BLOCK if build system consumed input
        if (ActionLock.ConsumeInputThisFrame)
        {
            clickHeld = false;
            return;
        }

        if (BuildManager.Instance != null && BuildManager.Instance.IsPlacing)
        {
            clickHeld = false;
            return;
        }

        if (!clickHeld)
            return;

        HandleDragPaint();
    }

    void HandleDragPaint()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camZ));

        worldPos.z = 0f;
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        cell.z = 0;

        Debug.Log($"[TiltManager] Cursor cell: {cell}");

        if (cell == lastCell)
        {
            Debug.Log("[TiltManager] Skipped - same cell as last frame");
            return;
        }

        lastCell = cell;

        if (removeMode)
        {
            Debug.Log($"[TiltManager] REMOVE attempt at {cell}");
            Remove(cell);
        }
        else
        {
            Debug.Log($"[TiltManager] PLACE attempt at {cell}");
            Place(cell);
        }
    }

    void ToggleRemoveMode(InputAction.CallbackContext ctx)
    {
        removeMode = !removeMode;
        Debug.Log($"[TiltManager] Remove mode toggled => {removeMode}");
    }

    bool IsShovelEquipped()
    {
        hotbar ??= PlayerHotbarManager.Instance;

        if (hotbar == null)
        {
            Debug.LogWarning("[TiltManager] Hotbar is NULL");
            return false;
        }

        ItemData item = hotbar.GetSelectedItem();

        bool result = item is UseableItem usable && usable.abilityToExecute is ShowelSO;


        return result;
    }

    void Place(Vector3Int pos)
    {
        if (tilemap.GetTile(pos) != null)
        {
            Debug.Log($"[TiltManager] PLACE blocked (tile exists) at {pos}");
            return;
        }

        tilemap.SetTile(pos, soilTile);
        placedTiles.Add(pos);

        Debug.Log($"[TiltManager] TILE PLACED at {pos}");

        UpdatePhysics();
        saveManager?.SaveAfterChange();
    }

    void Remove(Vector3Int pos)
    {
        if (tilemap.GetTile(pos) == null)
        {
            Debug.Log($"[TiltManager] REMOVE blocked (no tile) at {pos}");
            return;
        }

        tilemap.SetTile(pos, null);
        placedTiles.Remove(pos);

        Debug.Log($"[TiltManager] TILE REMOVED at {pos}");

        UpdatePhysics();
        saveManager?.SaveAfterChange();
    }

    void UpdatePhysics()
    {
        tilemap.RefreshAllTiles();

        if (compositeCollider != null)
        {
            compositeCollider.GenerateGeometry();
            Debug.Log("[TiltManager] Physics updated (CompositeCollider regenerated)");
        }
    }

    public List<SoilData> GetSaveData()
    {
        List<SoilData> data = new();


        foreach (Vector3Int pos in placedTiles)
        {
            data.Add(new SoilData { x = pos.x, y = pos.y, tileID = 0 });
        }


        return data;
    }

    public void LoadSoilTiles(List<SoilData> data)
    {
        tilemap.ClearAllTiles();
        placedTiles.Clear();


        if (data == null || data.Count == 0)
            return;

        foreach (SoilData s in data)
        {
            Vector3Int pos = new Vector3Int(s.x, s.y, 0);
            tilemap.SetTile(pos, soilTile);
            placedTiles.Add(pos);
        }

        UpdatePhysics();

    }
}