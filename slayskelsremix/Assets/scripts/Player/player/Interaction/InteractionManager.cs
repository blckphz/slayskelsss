using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class InteractionManager : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask interactLayer;
    public LayerMask largeStructureLayer;

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;

    [Header("Runtime Settings")]
    public bool restrictToInventory = false;

    public static bool MasterRestriction { get; private set; }
    public static bool IsBlockingBuildPreview { get; private set; }

    [Header("Delete Settings")]
    public float deconstructInterval = 0.05f;

    [Header("Tilemap")]
    public TileSaver tileSaver;

    [Header("Debug")]
    public bool enableDebugLogs = false;


    // =========================================================
    // REFERENCES
    // =========================================================

    private Camera mainCam;

    private GameObject currentHoverObj;

    private SpriteRenderer currentRenderer;

    private Tilemap currentTilemap;

    private TileTransparency currentTileTransparency;

    private Tilemap currentTileMapTarget;

    private Vector3Int currentTileCell;

    private TileBase currentTile;


    // =========================================================
    // ORIGINAL COLORS
    // =========================================================

    private Color originalSpriteColor;


    // =========================================================
    // DELETE
    // =========================================================

    private float deconstructCooldown;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        mainCam = Camera.main;

        if (tileSaver == null)
        {
            tileSaver =
                FindFirstObjectByType<TileSaver>();
        }
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private void OnValidate()
    {
        MasterRestriction =
            restrictToInventory;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // =====================================================
        // NOT IN BUILD MODE
        // =====================================================

        if (!BuildState.IsBuildMode)
        {
            ClearHighlight();
            ClearTileTarget();

            IsBlockingBuildPreview = false;

            return;
        }


        // =====================================================
        // CURRENTLY PLACING
        // =====================================================

        if (
            BuildManager.Instance != null &&
            BuildManager.Instance.IsPlacing)
        {
            ClearHighlight();
            ClearTileTarget();

            IsBlockingBuildPreview = false;

            return;
        }


        // =====================================================
        // INVENTORY RESTRICTION
        // =====================================================

        MasterRestriction =
            restrictToInventory;


        if (
            MasterRestriction &&
            !invUIToggle.IsInventoryOpen)
        {
            ClearHighlight();
            ClearTileTarget();

            IsBlockingBuildPreview = false;

            return;
        }


        // =====================================================
        // HOVER
        // =====================================================

        HandleHover();

        HandleTileHover();

        HandleHoldDelete();


        IsBlockingBuildPreview =
            currentHoverObj != null ||
            currentTileMapTarget != null;
    }


    // =========================================================
    // GET MOUSE WORLD POSITION
    // =========================================================

    private Vector2 GetMouseWorldPos()
    {
        if (mainCam == null)
            mainCam = Camera.main;


        if (Mouse.current == null)
            return Vector2.zero;


        Vector2 mousePos =
            Mouse.current.position.ReadValue();


        Vector3 world =
            mainCam.ScreenToWorldPoint(
                new Vector3(
                    mousePos.x,
                    mousePos.y,
                    Mathf.Abs(
                        mainCam.transform.position.z
                    )
                )
            );


        return new Vector2(
            world.x,
            world.y
        );
    }


    // =========================================================
    // HANDLE OBJECT HOVER
    // =========================================================

    private void HandleHover()
    {
        Vector2 point =
            GetMouseWorldPos();


        bool isShiftHeld =
            Keyboard.current != null &&
            Keyboard.current.shiftKey.isPressed;


        GameObject targetObj = null;


        // =====================================================
        // LARGE STRUCTURES
        // =====================================================

        if (isShiftHeld)
        {
            Collider2D largeHit =
                Physics2D.OverlapPoint(
                    point,
                    largeStructureLayer
                );


            if (largeHit != null)
            {
                targetObj =
                    largeHit.gameObject;
            }
        }


        // =====================================================
        // SMALL OBJECTS
        // =====================================================

        if (targetObj == null)
        {
            Collider2D smallHit =
                Physics2D.OverlapPoint(
                    point,
                    interactLayer
                );


            if (smallHit != null)
            {
                targetObj =
                    smallHit.gameObject;
            }
        }


        // =====================================================
        // TARGET HAS NOT CHANGED
        // =====================================================

        if (targetObj == currentHoverObj)
            return;


        ClearHighlight();


        if (targetObj == null)
            return;


        // =====================================================
        // CHECK INTERACTABLE
        // =====================================================

        objectHealth health =
            targetObj.GetComponent<objectHealth>();


        ChestInventory chest =
            targetObj.GetComponent<ChestInventory>();


        BuildIdentity build =
            targetObj.GetComponent<BuildIdentity>();


        if (
            health == null &&
            chest == null &&
            build == null)
        {
            return;
        }


        currentHoverObj =
            targetObj;


        // =====================================================
        // SPRITE
        // =====================================================

        currentRenderer =
            targetObj.GetComponent<SpriteRenderer>();


        // =====================================================
        // TILEMAP
        // =====================================================

        currentTilemap =
            targetObj.GetComponentInChildren<Tilemap>(
                true
            );


        // =====================================================
        // TILE TRANSPARENCY
        // =====================================================

        currentTileTransparency =
            targetObj.GetComponentInChildren<
                TileTransparency>(
                true
            );


        // =====================================================
        // SPRITE HIGHLIGHT
        // =====================================================

        if (currentRenderer != null)
        {
            originalSpriteColor =
                currentRenderer.color;


            Color newColor =
                highlightColor;


            newColor.a =
                originalSpriteColor.a;


            currentRenderer.color =
                newColor;
        }


        // =====================================================
        // TILE TRANSPARENCY
        // =====================================================

        if (currentTileTransparency != null)
        {
            currentTileTransparency.MouseEntered();
        }
    }


    // =========================================================
    // HANDLE TILE HOVER
    // =========================================================

    private void HandleTileHover()
    {
        // Clear previous tile target
        currentTileMapTarget = null;
        currentTile = null;


        if (tileSaver == null)
            return;


        if (tileSaver.tilemaps == null)
            return;


        Vector2 mouseWorld =
            GetMouseWorldPos();


        // =====================================================
        // CHECK ALL SAVED TILEMAPS
        // =====================================================

        foreach (
            Tilemap tilemap
            in tileSaver.tilemaps)
        {
            if (tilemap == null)
                continue;


            Vector3Int cell =
                tilemap.WorldToCell(
                    mouseWorld
                );


            TileBase tile =
                tilemap.GetTile(cell);


            if (tile == null)
                continue;


            // =================================================
            // TILE FOUND
            // =================================================

            currentTileMapTarget =
                tilemap;

            currentTileCell =
                cell;

            currentTile =
                tile;


            return;
        }
    }


    // =========================================================
    // HOLD DELETE
    // =========================================================

    private void HandleHoldDelete()
    {
        if (
            Mouse.current == null ||
            !Mouse.current.rightButton.isPressed)
        {
            deconstructCooldown = 0f;

            return;
        }


        deconstructCooldown -=
            Time.deltaTime;


        if (deconstructCooldown > 0f)
            return;


        // =====================================================
        // TILE FIRST
        // =====================================================

        if (currentTileMapTarget != null)
        {
            DeleteCurrentTile();

            deconstructCooldown =
                deconstructInterval;

            return;
        }


        // =====================================================
        // NORMAL OBJECT
        // =====================================================

        if (currentHoverObj == null)
            return;


        DeleteCurrentObject();

        deconstructCooldown =
            deconstructInterval;
    }


    // =========================================================
    // DELETE CURRENT TILE
    // =========================================================

    private void DeleteCurrentTile()
    {
        if (currentTileMapTarget == null)
            return;


        TileBase tile =
            currentTileMapTarget.GetTile(
                currentTileCell
            );


        if (tile == null)
            return;


        // =====================================================
        // FIND BUILD ITEM
        // =====================================================

        buildSO build =
            FindBuildItemForTile(
                tile
            );


        // =====================================================
        // RETURN ITEM
        // =====================================================

        if (
            build != null &&
            InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(
                build,
                1
            );
        }


        // =====================================================
        // REMOVE TILE
        // =====================================================

        currentTileMapTarget.SetTile(
            currentTileCell,
            null
        );


        // =====================================================
        // REFRESH
        // =====================================================

        currentTileMapTarget.RefreshTile(
            currentTileCell
        );


        CompositeCollider2D collider =
            currentTileMapTarget.GetComponent<
                CompositeCollider2D
            >();


        if (collider != null)
        {
            collider.GenerateGeometry();
        }


        // =====================================================
        // SAVE
        // =====================================================

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance
                .SaveAfterChange();
        }


        // =====================================================
        // CLEAR
        // =====================================================

        ClearTileTarget();
    }


    // =========================================================
    // FIND BUILD ITEM FOR TILE
    // =========================================================

    private buildSO FindBuildItemForTile(
        TileBase targetTile)
    {
        if (targetTile == null)
            return null;


        if (tileSaver == null)
            return null;


        // =====================================================
        // SEARCH DATABASE THROUGH BUILD MANAGER ITEMS
        // =====================================================

        BuildingSaveManager saveManager =
            BuildingSaveManager.Instance;


        if (saveManager == null)
            return null;


        ItemDatabase database =
            saveManager.database;


        if (database == null)
            return null;


        // =====================================================
        // TOOLS
        // =====================================================

        buildSO result =
            FindBuildItemInList(
                database.Tools,
                targetTile
            );


        if (result != null)
            return result;


        // =====================================================
        // PLACEABLES
        // =====================================================

        result =
            FindBuildItemInList(
                database.Placeables,
                targetTile
            );


        if (result != null)
            return result;


        // =====================================================
        // RESOURCES
        // =====================================================

        result =
            FindBuildItemInList(
                database.Resources,
                targetTile
            );


        if (result != null)
            return result;


        return null;
    }


    // =========================================================
    // FIND BUILD ITEM IN LIST
    // =========================================================

    private buildSO FindBuildItemInList(
        IEnumerable<ItemData> items,
        TileBase targetTile)
    {
        if (items == null)
            return null;


        foreach (
            ItemData item
            in items)
        {
            if (item is not buildSO build)
                continue;


            if (!build.baseTile)
                continue;


            if (build.placeablePrefab == null)
                continue;


            Tilemap prefabTilemap =
                build.placeablePrefab
                    .GetComponentInChildren<Tilemap>(
                        true
                    );


            if (prefabTilemap == null)
                continue;


            BoundsInt bounds =
                prefabTilemap.cellBounds;


            foreach (
                Vector3Int cell
                in bounds.allPositionsWithin)
            {
                TileBase prefabTile =
                    prefabTilemap.GetTile(
                        cell
                    );


                if (prefabTile == null)
                    continue;


                if (prefabTile == targetTile)
                {
                    return build;
                }
            }
        }


        return null;
    }


    // =========================================================
    // DELETE NORMAL OBJECT
    // =========================================================

    private void DeleteCurrentObject()
    {
        if (currentHoverObj == null)
            return;


        GameObject target =
            currentHoverObj;


        // =====================================================
        // BUILDING
        // =====================================================

        BuildIdentity build =
            target.GetComponent<BuildIdentity>();


        if (build != null)
        {
            // ================================================
            // RETURN ITEM
            // ================================================

            if (
                !build.GetsDestroyedByPlayer &&
                build.item != null &&
                InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(
                    build.item,
                    1
                );
            }


            // ================================================
            // REMOVE FROM SAVE MANAGER
            // ================================================

            if (
                BuildingSaveManager.Instance != null)
            {
                BuildingSaveManager.Instance
                    .UnregisterBuilding(
                        target
                    );
            }


            // ================================================
            // RESTORE ORIGINAL TILE
            // ================================================

            tileReplaceManager.Instance?.RestoreTile(
                build.cell
            );


            // ================================================
            // DESTROY
            // ================================================

            Destroy(target);


            // ================================================
            // SAVE
            // ================================================

            if (
                BuildingSaveManager.Instance != null)
            {
                BuildingSaveManager.Instance
                    .SaveAfterChange();
            }


            ClearHighlight();

            return;
        }


        // =====================================================
        // NORMAL OBJECT
        // =====================================================

        objectHealth health =
            target.GetComponent<objectHealth>();


        if (
            health != null &&
            health.IsPlayerInRange())
        {
            ChestInventory chest =
                target.GetComponent<ChestInventory>();


            // Do not destroy a chest containing items
            if (
                chest != null &&
                !chest.IsEmpty())
            {
                return;
            }


            health.Deconstruct();


            ClearHighlight();
        }
    }


    // =========================================================
    // CLEAR TILE TARGET
    // =========================================================

    private void ClearTileTarget()
    {
        currentTileMapTarget = null;

        currentTileCell =
            Vector3Int.zero;

        currentTile = null;
    }


    // =========================================================
    // CLEAR HIGHLIGHT
    // =========================================================

    private void ClearHighlight()
    {
        // =====================================================
        // RESTORE SPRITE
        // =====================================================

        if (currentRenderer != null)
        {
            currentRenderer.color =
                originalSpriteColor;
        }


        // =====================================================
        // TILE TRANSPARENCY
        // =====================================================

        if (currentTileTransparency != null)
        {
            currentTileTransparency.MouseExited();
        }


        // =====================================================
        // CLEAR
        // =====================================================

        currentHoverObj = null;

        currentRenderer = null;

        currentTilemap = null;

        currentTileTransparency = null;
    }
}