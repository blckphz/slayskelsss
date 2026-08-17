using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Pathfinding;

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

    public void Initialize(
        BuildManager buildManager)
    {
        manager =
            buildManager;
    }


    // =====================================================
    // INPUT
    // =====================================================

    public void SetInput(
        PlayerInputHandler playerInput)
    {
        input =
            playerInput;
    }


    // =====================================================
    // START PLACING
    // =====================================================

    public void StartPlacing(
        buildSO item)
    {
        if (
            item == null ||
            manager == null)
        {
            return;
        }


        if (previewObject != null)
        {
            Destroy(
                previewObject
            );
        }


        previewObject = null;
        ghost = null;
        previewRotation = null;

        currentItem =
            item;


        if (item.placeablePrefab == null)
        {
            Debug.LogError(
                "[BuildPlacer] " +
                item.name +
                " has no placeablePrefab."
            );

            return;
        }


        previewObject =
            Instantiate(
                item.placeablePrefab
            );


        ghost =
            previewObject.GetComponent<
                ghostBuildPreview
            >();


        previewRotation =
            previewObject.GetComponent<
                RotateBuildables
            >();


        if (ghost != null)
        {
            ghost.placementMask =
                manager.placementMask;

            ghost.footprint =
                item.size;


            if (item.baseTile)
            {
                Tilemap targetTilemap =
                    GetTilemapForItem(
                        item
                    );


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
            else
            {
                ghost.grid =
                    manager.buildGrid;
            }


            ghost.InitializeGhost();
        }
    }


    // =====================================================
    // PROCESS
    // =====================================================

    public void ProcessPlacement()
    {
        if (previewObject == null)
            return;


        MovePreview();


        if (
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            previewRotation?.ToggleSprite();
        }


        bool isUI =
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();


        lastCanPlace =
            !isUI &&
            CanPlace();


        UpdateColor(
            isUI,
            lastCanPlace
        );


        if (
            lastCanPlace &&
            input != null &&
            input.LeftClickPressed())
        {
            TryPlace();
        }
    }


    // =====================================================
    // TILEMAP LOOKUP
    // =====================================================

    private Tilemap GetTilemapForItem(
        buildSO item)
    {
        if (
            item == null ||
            !item.baseTile)
        {
            return null;
        }


        string tilemapName =
            item.baseTileName;


        if (
            string.IsNullOrEmpty(
                tilemapName))
        {
            return null;
        }


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
        }


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


        Debug.LogWarning(
            "[BuildPlacer] Could not find Tilemap: " +
            tilemapName
        );


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


        world.z =
            0f;


        Vector3 position;


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


                cell.z =
                    0;


                position =
                    targetTilemap.GetCellCenterWorld(
                        cell
                    );


                position.z =
                    0f;


                previewObject.transform.position =
                    position;


                return;
            }
        }


        if (
            BuildState.UseGridPlacement &&
            manager.buildGrid != null)
        {
            Vector3Int cell =
                manager.buildGrid.WorldToCell(
                    world
                );


            cell.z =
                0;


            position =
                manager.buildGrid.GetCellCenterWorld(
                    cell
                );


            position.z =
                0f;
        }
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


        if (currentItem.baseTile)
        {
            Tilemap targetTilemap =
                GetTilemapForItem(
                    currentItem
                );


            if (targetTilemap == null)
                return;


            cell =
                targetTilemap.WorldToCell(
                    position
                );


            cell.z =
                0;


            PlaceBaseTile(
                cell,
                position,
                targetTilemap
            );


            return;
        }


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


        cell.z =
            0;


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
        if (
            manager == null ||
            currentItem == null ||
            targetTilemap == null)
        {
            return;
        }


        if (currentItem.placeablePrefab == null)
        {
            Debug.LogError(
                "[BuildPlacer] " +
                currentItem.name +
                " has no placeablePrefab."
            );

            return;
        }


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


        BoundsInt bounds =
            sourceTilemap.cellBounds;


        Vector3Int sourceOrigin =
            bounds.min;


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


            targetCell.z =
                0;


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


        if (placedCells.Count == 0)
        {
            Debug.LogError(
                "[BuildPlacer] Prefab contains no tiles."
            );

            return;
        }


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


            targetCell.z =
                0;


            targetTilemap.SetTile(
                targetCell,
                tile
            );
        }


        targetTilemap.RefreshAllTiles();


        // =================================================
        // ROOM SYSTEM
        // =================================================

        RegisterWithRoomSystem(
            currentItem,
            placedCells
        );


        // =================================================
        // EFFECTS
        // =================================================

        PlayPlacementEffects(
            position
        );


        if (
            currentItem.placementSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(
                currentItem.placementSound
            );
        }


        if (
            BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance
                .SaveAfterChange();
        }


        ConsumeAfterPlacement(
            currentItem
        );
    }


    // =====================================================
    // PLACE PREFAB
    // =====================================================

    private void PlacePrefab(
        Vector3 position,
        Vector3Int cell)
    {
        if (currentItem == null)
            return;


        buildSO placedItem =
            currentItem;


        // =================================================
        // INSTANTIATE
        // =================================================

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
            obj.GetComponent<
                RotateBuildables
            >();


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
            obj.GetComponent<
                BuildIdentity
            >();


        if (identity == null)
        {
            identity =
                obj.AddComponent<
                    BuildIdentity
                >();
        }


        identity.item =
            placedItem;

        identity.cell =
            cell;


        // =================================================
        // ⭐ WALL POSITION FIX
        // =================================================
        //
        // DO NOT let WallBehav calculate its position from
        // the prefab's child Tilemap.
        //
        // The actual build grid cell is already known here.
        //
        // =================================================

        WallBehav wall =
            obj.GetComponent<
                WallBehav
            >();


        if (wall != null)
        {
            wall.SetGridPosition(
                cell
            );
        }


        // =================================================
        // SOIL
        // =================================================

        if (
            SoilOccupancyManager.Instance != null)
        {
            SoilOccupancyManager.Instance.Register(
                cell,
                identity
            );
        }


        // =================================================
        // TILE REPLACEMENT
        // =================================================

        if (
            tileReplaceManager.Instance != null)
        {
            tileReplaceManager.Instance.ReplaceTile(
                position,
                placedItem
            );
        }


        // =================================================
        // ROOM SYSTEM
        // =================================================

        RegisterWithRoomSystem(
            placedItem,
            new List<Vector3Int>
            {
                cell
            }
        );


        // =================================================
        // SAVE
        // =================================================

        if (
            BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance
                .RegisterBuilding(
                    obj
                );

            BuildingSaveManager.Instance
                .SaveAfterChange();
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

        ConsumeAfterPlacement(
            placedItem
        );
    }


    // =====================================================
    // ROOM SYSTEM REGISTRATION
    // =====================================================

    private void RegisterWithRoomSystem(
        buildSO item,
        List<Vector3Int> cells)
    {
        if (
            BaseTilePlacementManager.Instance == null)
        {
            Debug.LogError(
                "<color=red>" +
                "[ROOM SYSTEM ERROR] " +
                "BaseTilePlacementManager.Instance is NULL!" +
                "</color>"
            );

            return;
        }


        if (item == null)
            return;


        if (
            cells == null ||
            cells.Count == 0)
        {
            return;
        }


        if (
            item.category != TileCategory.Floor &&
            item.category != TileCategory.Wall &&
            item.category != TileCategory.Door)
        {
            return;
        }


        Debug.Log(
            $"<color=yellow>" +
            $"[ROOM SYSTEM] REGISTERING " +
            $"{item.category}" +
            $"</color> | " +
            $"Item: {item.name} | " +
            $"Cells: {cells.Count}"
        );


        BaseTilePlacementManager.Instance.Register(
            item,
            cells
        );
    }


    // =====================================================
    // CONSUME
    // =====================================================

    private void ConsumeAfterPlacement(
        buildSO placedItem)
    {
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
    // NEW PREVIEW
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


        Tilemap sourceTilemap =
            currentItem.placeablePrefab
                .GetComponentInChildren<Tilemap>(
                    true
                );


        if (sourceTilemap == null)
            return false;


        Vector3Int origin =
            targetTilemap.WorldToCell(
                previewObject.transform.position
            );


        origin.z =
            0;


        BoundsInt bounds =
            sourceTilemap.cellBounds;


        Vector3Int sourceOrigin =
            bounds.min;


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


            targetCell.z =
                0;


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
    // COLOR
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