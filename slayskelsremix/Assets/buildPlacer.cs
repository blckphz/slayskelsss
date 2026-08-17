using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildPlacer : MonoBehaviour
{
    private BuildManager manager;

    private GameObject previewObject;
    private buildSO currentItem;

    private ghostBuildPreview ghost;
    private RotateBuildables previewRotation;

    private PlayerInputHandler input;

    private bool lastCanPlace;


    // =====================================================
    // CURRENT ITEM
    // =====================================================

    public buildSO GetCurrentItem()
    {
        return currentItem;
    }


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(BuildManager buildManager)
    {
        manager = buildManager;
    }


    // =====================================================
    // INPUT
    // =====================================================

    public void SetInput(PlayerInputHandler playerInput)
    {
        input = playerInput;
    }


    // =====================================================
    // START PLACING
    // =====================================================

    public void StartPlacing(buildSO item)
    {
        if (item == null || manager == null)
            return;


        // =================================================
        // DESTROY PREVIOUS PREVIEW
        // =================================================

        if (previewObject != null)
        {
            Destroy(previewObject);
        }


        previewObject = null;
        ghost = null;
        previewRotation = null;


        currentItem = item;


        // =================================================
        // PREFAB CHECK
        // =================================================

        if (item.placeablePrefab == null)
        {
            Debug.LogError(
                "[BuildPlacer] " +
                item.name +
                " has no placeablePrefab."
            );

            return;
        }


        // =================================================
        // CREATE PREVIEW
        // =================================================

        previewObject =
            Instantiate(
                item.placeablePrefab
            );


        // =================================================
        // GET GHOST
        // =================================================

        ghost =
            previewObject.GetComponent<ghostBuildPreview>();


        // =================================================
        // GET ROTATION
        // =================================================

        previewRotation =
            previewObject.GetComponent<RotateBuildables>();


        // =================================================
        // INITIALIZE GHOST
        // =================================================

        if (ghost != null)
        {
            ghost.placementMask =
                manager.placementMask;

            ghost.footprint =
                item.size;


            // =================================================
            // TILE ITEM
            // =================================================

            if (item.baseTile)
            {
                Tilemap targetTilemap =
                    GetTilemapForItem(item);

                if (targetTilemap != null)
                {
                    ghost.grid =
                        targetTilemap.layoutGrid;
                }
                else
                {
                    ghost.grid =
                        manager.buildGrid;
                }
            }


            // =================================================
            // NORMAL BUILDING
            // =================================================

            else
            {
                ghost.grid =
                    manager.buildGrid;
            }


            ghost.InitializeGhost();
        }
    }


    // =====================================================
    // PROCESS PLACEMENT
    // =====================================================

    public void ProcessPlacement()
    {
        if (previewObject == null)
            return;


        // =================================================
        // MOVE
        // =================================================

        MovePreview();


        // =================================================
        // ROTATE
        // =================================================

        if (
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            previewRotation?.ToggleSprite();
        }


        // =================================================
        // UI CHECK
        // =================================================

        bool isUI =
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();


        // =================================================
        // CAN PLACE
        // =================================================

        lastCanPlace =
            !isUI &&
            CanPlace();


        // =================================================
        // COLOR
        // =================================================

        UpdateColor(
            isUI,
            lastCanPlace
        );


        // =================================================
        // PLACE
        // =================================================

        if (
            lastCanPlace &&
            input != null &&
            input.LeftClickPressed())
        {
            TryPlace();
        }
    }


    // =====================================================
    // GET TILEMAP FOR ITEM
    // =====================================================

    private Tilemap GetTilemapForItem(
        buildSO item)
    {
        if (item == null)
            return null;


        if (!item.baseTile)
            return null;


        string tilemapName =
            item.baseTileName;


        if (string.IsNullOrEmpty(tilemapName))
            return null;


        // =================================================
        // USE TILE SAVER TILEMAP LIST
        // =================================================

        if (
            TileSaver.Instance != null &&
            TileSaver.Instance.tilemaps != null)
        {
            foreach (
                Tilemap tilemap
                in TileSaver.Instance.tilemaps)
            {
                if (tilemap == null)
                    continue;


                // Exact Tilemap name
                if (
                    tilemap.name ==
                    tilemapName)
                {
                    return tilemap;
                }


                // GameObject name
                if (
                    tilemap.gameObject.name ==
                    tilemapName)
                {
                    return tilemap;
                }
            }
        }


        // =================================================
        // FALLBACK: SEARCH SCENE
        // =================================================

        Tilemap[] tilemaps =
            FindObjectsByType<Tilemap>(
                FindObjectsSortMode.None
            );


        foreach (
            Tilemap tilemap
            in tilemaps)
        {
            if (tilemap == null)
                continue;


            if (
                tilemap.name ==
                tilemapName)
            {
                return tilemap;
            }


            if (
                tilemap.gameObject.name ==
                tilemapName)
            {
                return tilemap;
            }
        }


        return null;
    }


    // =====================================================
    // MOVE PREVIEW
    // =====================================================

    private void MovePreview()
    {
        if (
            input == null ||
            previewObject == null ||
            manager == null ||
            manager.playerCamera == null)
        {
            return;
        }


        Vector2 mouse =
            input.GetMousePosition();


        Vector3 world =
            manager.playerCamera.ScreenToWorldPoint(
                new Vector3(
                    mouse.x,
                    mouse.y,
                    Mathf.Abs(
                        manager.playerCamera.transform.position.z
                    )
                )
            );


        world.z = 0f;


        Vector3 position;


        // =================================================
        // TILE ITEM
        // =================================================

        if (
            currentItem != null &&
            currentItem.baseTile)
        {
            Tilemap targetTilemap =
                GetTilemapForItem(
                    currentItem
                );


            if (targetTilemap != null)
            {
                Vector3Int cell =
                    targetTilemap.WorldToCell(
                        world
                    );


                cell.z = 0;


                position =
                    targetTilemap.GetCellCenterWorld(
                        cell
                    );


                position.z = 0f;


                previewObject.transform.position =
                    position;


                return;
            }
        }


        // =================================================
        // NORMAL GRID
        // =================================================

        if (
            BuildState.UseGridPlacement &&
            manager.buildGrid != null)
        {
            Vector3Int cell =
                manager.buildGrid.WorldToCell(
                    world
                );


            cell.z = 0;


            position =
                manager.buildGrid.GetCellCenterWorld(
                    cell
                );


            position.z = 0f;
        }


        // =================================================
        // FREE PLACEMENT
        // =================================================

        else
        {
            position =
                new Vector3(
                    world.x,
                    world.y,
                    0f
                );
        }


        previewObject.transform.position =
            position;
    }


    // =====================================================
    // TRY PLACE
    // =====================================================

    private void TryPlace()
    {
        if (
            previewObject == null ||
            currentItem == null)
        {
            return;
        }


        Vector3 position =
            previewObject.transform.position;


        Vector3Int cell;


        // =================================================
        // TILE ITEM
        // =================================================

        if (currentItem.baseTile)
        {
            Tilemap targetTilemap =
                GetTilemapForItem(
                    currentItem
                );


            if (targetTilemap == null)
            {
                Debug.LogError(
                    "[BuildPlacer] No Tilemap found for '" +
                    currentItem.baseTileName +
                    "'."
                );

                return;
            }


            cell =
                targetTilemap.WorldToCell(
                    position
                );


            cell.z = 0;


            PlaceBaseTile(
                cell,
                position,
                targetTilemap
            );


            return;
        }


        // =================================================
        // NORMAL BUILDING
        // =================================================

        if (manager.buildGrid != null)
        {
            cell =
                manager.buildGrid.WorldToCell(
                    position
                );
        }
        else
        {
            cell =
                Vector3Int.zero;
        }


        cell.z = 0;


        PlacePrefab(
            position,
            cell
        );
    }


    // =====================================================
    // PLACE BASE TILE
    // =====================================================

    private void PlaceBaseTile(
        Vector3Int cell,
        Vector3 position,
        Tilemap targetTilemap)
    {
        if (manager == null)
            return;


        if (currentItem == null)
            return;


        if (targetTilemap == null)
            return;


        if (currentItem.placeablePrefab == null)
        {
            Debug.LogError(
                "[BuildPlacer] " +
                currentItem.name +
                " has no placeablePrefab."
            );

            return;
        }


        // =================================================
        // GET SOURCE TILEMAP FROM PREFAB
        // =================================================

        Tilemap sourceTilemap =
            currentItem.placeablePrefab
                .GetComponentInChildren<Tilemap>(
                    true
                );


        if (sourceTilemap == null)
        {
            Debug.LogError(
                "[BuildPlacer] " +
                currentItem.placeablePrefab.name +
                " has no Tilemap."
            );

            return;
        }


        // =================================================
        // SOURCE BOUNDS
        // =================================================

        BoundsInt bounds =
            sourceTilemap.cellBounds;


        Vector3Int sourceOrigin =
            bounds.min;


        // =================================================
        // COLLECT CELLS
        // =================================================

        List<Vector3Int> placedCells =
            new List<Vector3Int>();


        foreach (
            Vector3Int sourceCell
            in bounds.allPositionsWithin)
        {
            TileBase tile =
                sourceTilemap.GetTile(
                    sourceCell
                );


            if (tile == null)
                continue;


            Vector3Int localCell =
                sourceCell -
                sourceOrigin;


            Vector3Int targetCell =
                cell +
                localCell;


            targetCell.z = 0;


            // =================================================
            // CHECK EXISTING TILE
            // =================================================

            if (
                targetTilemap.GetTile(
                    targetCell
                ) != null)
            {
                return;
            }


            placedCells.Add(
                targetCell
            );
        }


        // =================================================
        // NO TILES
        // =================================================

        if (placedCells.Count == 0)
        {
            Debug.LogError(
                "[BuildPlacer] Prefab contains no tiles."
            );

            return;
        }


        // =================================================
        // PLACE TILES
        // =================================================

        foreach (
            Vector3Int sourceCell
            in bounds.allPositionsWithin)
        {
            TileBase tile =
                sourceTilemap.GetTile(
                    sourceCell
                );


            if (tile == null)
                continue;


            Vector3Int localCell =
                sourceCell -
                sourceOrigin;


            Vector3Int targetCell =
                cell +
                localCell;


            targetCell.z = 0;


            targetTilemap.SetTile(
                targetCell,
                tile
            );
        }


        // =================================================
        // REFRESH TARGET TILEMAP
        // =================================================

        targetTilemap.RefreshAllTiles();


        // =================================================
        // REGISTER
        // =================================================

        if (
            BaseTilePlacementManager.Instance != null)
        {
            BaseTilePlacementManager.Instance.Register(
                currentItem,
                placedCells
            );
        }


        // =================================================
        // EFFECTS
        // =================================================

        PlayPlacementEffects(
            position
        );


        // =================================================
        // SOUND
        // =================================================

        if (
            currentItem.placementSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(
                currentItem.placementSound
            );
        }


        // =================================================
        // SAVE
        // =================================================

        if (
            BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.SaveAfterChange();
        }


        // =================================================
        // SAVE ITEM
        // =================================================

        buildSO placedItem =
            currentItem;


        // =================================================
        // CONSUME
        // =================================================

        PlayerHotbarManager hotbar =
            PlayerHotbarManager.Instance;


        if (hotbar != null)
        {
            hotbar.UseSelectedStack(
                placedItem.consumeAmount
            );


            ItemData selected =
                hotbar.GetSelectedItem();


            if (selected == placedItem)
            {
                StartNewPreview(
                    placedItem
                );

                return;
            }
        }


        // =================================================
        // NO MORE ITEMS
        // =================================================

        DestroyPreviewAfterPlacement();
    }


    // =====================================================
    // PLACE NORMAL PREFAB
    // =====================================================

    private void PlacePrefab(
        Vector3 position,
        Vector3Int cell)
    {
        if (currentItem == null)
            return;


        buildSO placedItem =
            currentItem;


        GameObject obj =
            Instantiate(
                placedItem.placeablePrefab,
                position,
                Quaternion.identity
            );


        // =================================================
        // ROTATION
        // =================================================

        RotateBuildables placedRotation =
            obj.GetComponent<RotateBuildables>();


        if (
            previewRotation != null &&
            placedRotation != null)
        {
            placedRotation.ApplyState(
                previewRotation.UsingSecondSprite
            );
        }


        // =================================================
        // BUILD IDENTITY
        // =================================================

        BuildIdentity identity =
            obj.GetComponent<BuildIdentity>();


        if (identity == null)
        {
            identity =
                obj.AddComponent<BuildIdentity>();
        }


        identity.item =
            placedItem;


        identity.cell =
            cell;


        // =================================================
        // SOIL
        // =================================================

        SoilOccupancyManager.Instance?.Register(
            cell,
            identity
        );


        // =================================================
        // TILE REPLACEMENT
        // =================================================

        tileReplaceManager.Instance?.ReplaceTile(
            position,
            placedItem
        );


        // =================================================
        // SAVE
        // =================================================

        if (
            BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(
                obj
            );

            BuildingSaveManager.Instance.SaveAfterChange();
        }


        // =================================================
        // PATHFINDING
        // =================================================

        UpdatePathfinding(
            position
        );


        // =================================================
        // EFFECTS
        // =================================================

        PlayPlacementEffects(
            position
        );


        // =================================================
        // SOUND
        // =================================================

        if (
            placedItem.placementSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(
                placedItem.placementSound
            );
        }


        // =================================================
        // CONSUME
        // =================================================

        PlayerHotbarManager hotbar =
            PlayerHotbarManager.Instance;


        if (hotbar != null)
        {
            hotbar.UseSelectedStack(
                placedItem.consumeAmount
            );


            ItemData selected =
                hotbar.GetSelectedItem();


            if (selected == placedItem)
            {
                StartNewPreview(
                    placedItem
                );

                return;
            }
        }


        DestroyPreviewAfterPlacement();
    }


    // =====================================================
    // START NEW PREVIEW
    // =====================================================

    private void StartNewPreview(
        buildSO item)
    {
        if (item == null)
            return;


        if (previewObject != null)
        {
            Destroy(
                previewObject
            );
        }


        previewObject = null;
        ghost = null;
        previewRotation = null;


        StartPlacing(
            item
        );
    }


    // =====================================================
    // PATHFINDING
    // =====================================================

    private void UpdatePathfinding(
        Vector3 position)
    {
        if (
            manager == null ||
            manager.astar == null ||
            AstarPath.active == null)
        {
            return;
        }


        float size =
            Mathf.Max(
                currentItem.size.x,
                currentItem.size.y
            );


        Bounds bounds =
            new Bounds(
                position,
                Vector3.one * size
            );


        AstarPath.active.UpdateGraphs(
            bounds
        );
    }


    // =====================================================
    // EFFECTS
    // =====================================================

    private void PlayPlacementEffects(
        Vector3 position)
    {
        if (
            manager.shakeOnPlace &&
            CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(
                manager.buildShakeIntensity,
                manager.buildShakeDuration
            );
        }


        if (
            manager.placementEffectPrefab != null)
        {
            GameObject fx =
                Instantiate(
                    manager.placementEffectPrefab,
                    position + Vector3.up * 0.1f,
                    Quaternion.identity
                );


            Destroy(
                fx,
                2f
            );
        }
    }


    // =====================================================
    // CAN PLACE
    // =====================================================

    private bool CanPlace()
    {
        if (ghost == null)
            return false;


        // =================================================
        // WATER
        // =================================================

        Collider2D water =
            Physics2D.OverlapPoint(
                previewObject.transform.position,
                manager.waterLayer
            );


        if (
            water != null &&
            !currentItem.baseTile)
        {
            return false;
        }


        // =================================================
        // OBSTACLES
        // =================================================

        List<Collider2D> obstacles =
            ghost.GetObstacles();


        foreach (
            Collider2D hit
            in obstacles)
        {
            if (hit == null)
                continue;


            if (hit.CompareTag("Player"))
                continue;


            return false;
        }


        // =================================================
        // TILE CHECK
        // =================================================

        if (
            currentItem != null &&
            currentItem.baseTile)
        {
            if (!CanPlaceBaseTile())
                return false;
        }


        return true;
    }


    // =====================================================
    // CAN PLACE BASE TILE
    // =====================================================

    private bool CanPlaceBaseTile()
    {
        if (currentItem == null)
            return false;


        if (currentItem.placeablePrefab == null)
            return false;


        Tilemap targetTilemap =
            GetTilemapForItem(
                currentItem
            );


        if (targetTilemap == null)
            return false;


        // =================================================
        // SOURCE TILEMAP
        // =================================================

        Tilemap sourceTilemap =
            currentItem.placeablePrefab
                .GetComponentInChildren<Tilemap>(
                    true
                );


        if (sourceTilemap == null)
            return false;


        // =================================================
        // ORIGIN
        // =================================================

        Vector3Int origin =
            targetTilemap.WorldToCell(
                previewObject.transform.position
            );


        origin.z = 0;


        // =================================================
        // BOUNDS
        // =================================================

        BoundsInt bounds =
            sourceTilemap.cellBounds;


        Vector3Int sourceOrigin =
            bounds.min;


        // =================================================
        // CHECK TILES
        // =================================================

        foreach (
            Vector3Int sourceCell
            in bounds.allPositionsWithin)
        {
            TileBase tile =
                sourceTilemap.GetTile(
                    sourceCell
                );


            if (tile == null)
                continue;


            Vector3Int localCell =
                sourceCell -
                sourceOrigin;


            Vector3Int targetCell =
                origin +
                localCell;


            targetCell.z = 0;


            if (
                targetTilemap.GetTile(
                    targetCell
                ) != null)
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // UPDATE GHOST COLOR
    // =====================================================

    private void UpdateColor(
        bool blockedUI,
        bool canPlace)
    {
        if (ghost == null)
            return;


        if (blockedUI)
        {
            ghost.SetColor(
                new Color(
                    1f,
                    0f,
                    0f,
                    0.2f
                )
            );

            return;
        }


        if (canPlace)
        {
            ghost.SetColor(
                Color.green
            );
        }
        else
        {
            ghost.SetColor(
                Color.red
            );
        }
    }


    // =====================================================
    // CANCEL
    // =====================================================

    public void Cancel()
    {
        if (previewObject != null)
        {
            Destroy(
                previewObject
            );
        }


        previewObject = null;
        ghost = null;
        previewRotation = null;
        currentItem = null;
        lastCanPlace = false;
    }


    // =====================================================
    // DESTROY PREVIEW
    // =====================================================

    private void DestroyPreviewAfterPlacement()
    {
        if (previewObject != null)
        {
            Destroy(
                previewObject
            );
        }


        previewObject = null;
        ghost = null;
        previewRotation = null;
        currentItem = null;
        lastCanPlace = false;
    }
}