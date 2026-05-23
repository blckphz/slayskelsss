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
        Debug.Log($"[TiltManager] Awake. CompositeCollider found: {compositeCollider != null}");
    }

    void OnEnable()
    {
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
        clickAction.Disable();
        if (removeSoil != null)
        {
            removeSoil.action.performed -= ToggleRemoveMode;
            removeSoil.action.Disable();
        }
    }

    void Update()
    {
        if (!clickAction.IsPressed() || !BuildState.IsBuildMode || !IsShovelEquipped()) return;
        HandleTileInput();
    }

    void ToggleRemoveMode(InputAction.CallbackContext ctx)
    {
        removeMode = !removeMode;
        Debug.Log("[TiltManager] Remove mode toggled: " + removeMode);
    }

    void HandleTileInput()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camZ));

        // Force Z to 0 so the calculation matches the grid
        worldPos.z = 0f;
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        cell.z = 0;

        TileBase t = tilemap.GetTile(cell);
        bool inHashSet = placedTiles.Contains(cell);

        Debug.Log($"[INPUT] Cell: {cell} | Tilemap tile: {(t != null ? t.name : "NULL")} | In HashSet: {inHashSet}");

        if (removeMode) Remove(cell);
        else Place(cell);
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
        pos.z = 0; // Force Z to 0
        if (tilemap.GetTile(pos) != null)
        {
            Debug.LogWarning($"[PLACE] Blocked: Tile already exists at {pos}");
            return;
        }

        tilemap.SetTile(pos, soilTile);
        placedTiles.Add(pos);
        Debug.Log($"[PLACE] Success at {pos}. New HashSet Count: {placedTiles.Count}");

        UpdatePhysics();
        saveManager?.SaveAfterChange();
    }

    void Remove(Vector3Int pos)
    {
        pos.z = 0; // Force Z to 0
        if (tilemap.GetTile(pos) == null)
        {
            Debug.LogWarning($"[REMOVE] Blocked: No tile to remove at {pos}");
            return;
        }

        tilemap.SetTile(pos, null);
        placedTiles.Remove(pos);
        Debug.Log($"[REMOVE] Success at {pos}. New HashSet Count: {placedTiles.Count}");

        UpdatePhysics();
        saveManager?.SaveAfterChange();
    }

    private void UpdatePhysics()
    {
        tilemap.RefreshAllTiles();
        if (compositeCollider != null)
        {
            compositeCollider.GenerateGeometry();
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

        if (data == null || data.Count == 0) return;

        foreach (SoilData s in data)
        {
            Vector3Int pos = new Vector3Int(s.x, s.y, 0);
            tilemap.SetTile(pos, soilTile);
            placedTiles.Add(pos);
        }

        UpdatePhysics();
        Debug.Log($"[LOAD] Complete. Total tiles in HashSet: {placedTiles.Count}");
    }
}