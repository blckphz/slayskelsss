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


        currentItem =
            item;


        if (item.placeablePrefab == null)
        {
            Debug.LogError(
                "[BuildPlacer] " +
                item.name +
                " has no placeablePrefab assigned."
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


            if (
                item.baseTile &&
                manager.baseTilemap != null)
            {
                ghost.grid =
                    manager.baseTilemap.layoutGrid;
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
    // PROCESS PLACEMENT
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
    // MOVE PREVIEW
    // =====================================================

    private void MovePreview()
    {
        if (
            input == null ||
            previewObject == null ||
            manager == null)
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


        // =================================================
        // BASE TILE
        // =================================================

        if (
            currentItem != null &&
            currentItem.baseTile &&
            manager.baseTilemap != null)
        {
            Vector3Int cell =
                manager.baseTilemap.WorldToCell(
                    world
                );


            cell.z =
                0;


            position =
                manager.baseTilemap.GetCellCenterWorld(
                    cell
                );


            position.z =
                0f;
        }


        // =================================================
        // NORMAL GRID
        // =================================================

        else if (
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
        // BASE TILE CELL
        // =================================================

        if (
            currentItem.baseTile &&
            manager.baseTilemap != null)
        {
            cell =
                manager.baseTilemap.WorldToCell(
                    position
                );
        }


        // =================================================
        // NORMAL BUILDING CELL
        // =================================================

        else if (
            manager.buildGrid != null)
        {
            cell =
                manager.buildGrid.WorldToCell(
                    position
                );
        }


        // =================================================
        // FALLBACK
        // =================================================

        else
        {
            cell =
                Vector3Int.zero;
        }


        cell.z =
            0;


        // =================================================
        // BASE TILE
        // =================================================

        if (currentItem.baseTile)
        {
            PlaceBaseTile(
                cell,
                position
            );

            return;
        }


        // =================================================
        // NORMAL BUILDING
        // =================================================

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
        Vector3 position)
    {
        if (
            manager == null ||
            manager.baseTilemap == null ||
            currentItem == null ||
            currentItem.baseTileAsset == null)
        {
            Debug.LogError(
                "[BuildPlacer] " +
                "Cannot place base tile."
            );

            return;
        }


        // =================================================
        // DON'T OVERWRITE
        // =================================================

        if (
            manager.baseTilemap.GetTile(cell) != null)
        {
            Debug.Log(
                "[BuildPlacer] " +
                "BASE TILE BLOCKED | " +
                "Cell=" +
                cell
            );

            return;
        }


        // =================================================
        // PAINT
        // =================================================

        manager.baseTilemap.SetTile(
            cell,
            currentItem.baseTileAsset
        );


        manager.baseTilemap.RefreshTile(
            cell
        );


        // =================================================
        // VERIFY
        // =================================================

        TileBase placedTile =
            manager.baseTilemap.GetTile(
                cell
            );


        if (placedTile == null)
        {
            Debug.LogError(
                "[BuildPlacer] " +
                "BASE TILE FAILED | " +
                "Cell=" +
                cell
            );

            return;
        }


        Debug.Log(
            "[BuildPlacer] BASE TILE PLACED | " +
            "Item=" +
            currentItem.name +
            " | ItemID=" +
            currentItem.itemID +
            " | Tile=" +
            placedTile.name +
            " | Cell=" +
            cell
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
            currentItem.placementSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(
                currentItem.placementSound
            );
        }


        // =================================================
        // SAVE IMMEDIATELY
        // =================================================

        if (
            BuildingSaveManager.Instance != null)
        {
            Debug.Log(
                "[BuildPlacer] " +
                "BASE TILE PLACED -> " +
                "CALLING SAVE"
            );


            BuildingSaveManager.Instance.SaveAfterChange();
        }
        else
        {
            Debug.LogError(
                "[BuildPlacer] " +
                "BuildingSaveManager.Instance " +
                "IS NULL!"
            );
        }


        // =================================================
        // SAVE ITEM BEFORE CONSUMING
        // =================================================

        buildSO placedItem =
            currentItem;


        PlayerHotbarManager hotbar =
            PlayerHotbarManager.Instance;


        // =================================================
        // CONSUME
        // =================================================

        if (hotbar != null)
        {
            hotbar.UseSelectedStack(
                placedItem.consumeAmount
            );
        }


        // =================================================
        // CHECK REMAINING
        // =================================================

        if (hotbar != null)
        {
            ItemData selected =
                hotbar.GetSelectedItem();


            if (selected == placedItem)
            {
                StartNewBaseTilePreview(
                    placedItem
                );

                return;
            }
        }


        // =================================================
        // NO MORE
        // =================================================

        DestroyPreviewAfterPlacement();
    }


    // =====================================================
    // NEW BASE TILE PREVIEW
    // =====================================================

    private void StartNewBaseTilePreview(
        buildSO item)
    {
        if (previewObject != null)
        {
            Destroy(
                previewObject
            );
        }


        previewObject =
            null;


        ghost =
            null;


        previewRotation =
            null;


        StartPlacing(
            item
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


        GameObject obj =
            Instantiate(
                currentItem.placeablePrefab,
                position,
                Quaternion.identity
            );


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
            currentItem;


        identity.cell =
            cell;


        SoilOccupancyManager.Instance?.Register(
            cell,
            identity
        );


        tileReplaceManager.Instance?.ReplaceTile(
            position,
            currentItem
        );


        // =================================================
        // SAVE BUILDING
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
            currentItem.placementSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(
                currentItem.placementSound
            );
        }


        // =================================================
        // HOTBAR
        // =================================================

        PlayerHotbarManager hotbar =
            PlayerHotbarManager.Instance;


        if (hotbar != null)
        {
            hotbar.UseSelectedStack(
                currentItem.consumeAmount
            );
        }


        // =================================================
        // DESTROY PREVIEW
        // =================================================

        DestroyPreviewAfterPlacement();
    }


    // =====================================================
    // PATHFINDING
    // =====================================================

    private void UpdatePathfinding(
        Vector3 position)
    {
        if (manager.astar == null)
            return;


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


        foreach (
            Collider2D hit
            in ghost.GetObstacles())
        {
            if (hit == null)
                continue;


            int layer =
                hit.gameObject.layer;


            if (hit.CompareTag("Player"))
                continue;


            if (
                (manager.interactLayer.value &
                (1 << layer)) != 0)
            {
                continue;
            }


            if (
                (manager.largeStructureLayer.value &
                (1 << layer)) != 0)
            {
                continue;
            }


            return false;
        }


        // =================================================
        // BASE TILE ALREADY EXISTS
        // =================================================

        if (
            currentItem != null &&
            currentItem.baseTile &&
            manager.baseTilemap != null)
        {
            Vector3Int cell =
                manager.baseTilemap.WorldToCell(
                    previewObject.transform.position
                );


            cell.z =
                0;


            if (
                manager.baseTilemap.GetTile(cell) != null)
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


        ghost.SetColor(
            canPlace
                ? Color.green
                : Color.red
        );
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


        previewObject =
            null;


        ghost =
            null;


        previewRotation =
            null;


        currentItem =
            null;


        lastCanPlace =
            false;
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


        previewObject =
            null;


        ghost =
            null;


        previewRotation =
            null;


        currentItem =
            null;


        lastCanPlace =
            false;
    }
}