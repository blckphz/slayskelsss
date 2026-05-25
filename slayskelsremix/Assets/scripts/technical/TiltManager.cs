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
    private bool clickQueued;

    private Vector3Int lastCell; // NEW: prevents re-paint spam

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

        // paint immediately on click
        clickQueued = true;
    }

    void OnClickUp(InputAction.CallbackContext ctx)
    {
        clickHeld = false;
        clickQueued = false;
    }

    void Update()
    {
        if (!BuildState.IsBuildMode || !IsShovelEquipped())
        {
            clickHeld = false;
            clickQueued = false;
            return;
        }

        // 🔒 HARD GLOBAL BLOCKS
        if (ActionLock.IsLocked)
        {
            clickQueued = false;
            return;
        }

        if (BuildManager.Instance != null && BuildManager.Instance.IsPlacing)
        {
            clickQueued = false;
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

        // 🔥 KEY FIX: only act when entering a NEW cell
        if (cell == lastCell)
            return;

        lastCell = cell;

        if (removeMode)
            Remove(cell);
        else
            Place(cell);
    }

    void ToggleRemoveMode(InputAction.CallbackContext ctx)
    {
        removeMode = !removeMode;
        Debug.Log("[TiltManager] Remove mode toggled: " + removeMode);
    }

    bool IsShovelEquipped()
    {
        hotbar ??= PlayerHotbarManager.Instance;
        if (hotbar == null) return false;

        ItemData item = hotbar.GetSelectedItem();
        return item is UseableItem usable && usable.abilityToExecute is ShowelSO;
    }

    void Place(Vector3Int pos)
    {
        if (tilemap.GetTile(pos) != null)
            return;

        tilemap.SetTile(pos, soilTile);
        placedTiles.Add(pos);

        UpdatePhysics();
        saveManager?.SaveAfterChange();

        Debug.Log($"[PLACE] {pos}");
    }

    void Remove(Vector3Int pos)
    {
        if (tilemap.GetTile(pos) == null)
            return;

        tilemap.SetTile(pos, null);
        placedTiles.Remove(pos);

        UpdatePhysics();
        saveManager?.SaveAfterChange();

        Debug.Log($"[REMOVE] {pos}");
    }

    void UpdatePhysics()
    {
        tilemap.RefreshAllTiles();

        if (compositeCollider != null)
            compositeCollider.GenerateGeometry();
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

        if (data == null || data.Count == 0) return;

        foreach (SoilData s in data)
        {
            Vector3Int pos = new Vector3Int(s.x, s.y, 0);
            tilemap.SetTile(pos, soilTile);
            placedTiles.Add(pos);
        }

        UpdatePhysics();
        Debug.Log($"[LOAD] Complete. Total tiles: {placedTiles.Count}");
    }
}