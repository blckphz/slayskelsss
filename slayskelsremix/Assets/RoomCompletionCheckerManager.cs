using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomCompletionCheckerManager : MonoBehaviour
{
    public static RoomCompletionCheckerManager Instance;

    [Header("Room Collision")]
    public Tilemap sourceTilemap;
    public Tilemap roomCollisionTilemap;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private static readonly Vector3Int[] AdjacentDirections =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
    };


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // =========================================================
    // DEBUG
    // =========================================================

    private void DebugLog(string message)
    {
        if (!enableDebugLogs)
            return;

        Debug.Log(
            "[ROOM SYSTEM] " +
            message
        );
    }


    // =========================================================
    // VALIDATE NEARBY FLOOR STRUCTURES
    // =========================================================

    public void ValidateNearbyFloorStructures(
        List<Vector3Int> changedCells,
        Dictionary<Vector3Int, BuildingStructure> cellToStructure)
    {
        if (
            changedCells == null ||
            changedCells.Count == 0)
        {
            return;
        }

        if (cellToStructure == null)
            return;


        HashSet<BuildingStructure> floorStructures =
            new HashSet<BuildingStructure>();


        foreach (var cell in changedCells)
        {
            foreach (var dir in AdjacentDirections)
            {
                Vector3Int neighborCell =
                    cell + dir;


                if (
                    cellToStructure.TryGetValue(
                        neighborCell,
                        out var structure
                    ) &&
                    structure != null &&
                    structure.itemType != null &&
                    structure.itemType.category ==
                        TileCategory.Floor)
                {
                    floorStructures.Add(
                        structure
                    );
                }
            }
        }


        DebugLog(
            $"Checking {floorStructures.Count} nearby floor structure(s)."
        );


        foreach (
            var floorStructure
            in floorStructures)
        {
            ValidateRoomEnclosure(
                floorStructure,
                cellToStructure
            );
        }
    }


    // =========================================================
    // VALIDATE ROOM ENCLOSURE
    // =========================================================

    public bool ValidateRoomEnclosure(
        BuildingStructure structure,
        Dictionary<Vector3Int, BuildingStructure> cellToStructure)
    {
        if (
            structure == null ||
            structure.cells == null ||
            structure.cells.Count == 0 ||
            structure.itemType == null ||
            structure.itemType.category != TileCategory.Floor)
        {
            return false;
        }


        if (cellToStructure == null)
            return false;


        structure.totalWallCount = 0;
        structure.totalDoorCount = 0;


        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;


        foreach (var cell in structure.cells)
        {
            if (cell.x < minX)
                minX = cell.x;

            if (cell.x > maxX)
                maxX = cell.x;

            if (cell.y < minY)
                minY = cell.y;

            if (cell.y > maxY)
                maxY = cell.y;
        }


        bool bottomClosed =
            CheckBottomBoundary(
                structure,
                minX,
                maxX,
                minY,
                cellToStructure
            );


        bool topClosed =
            CheckTopBoundary(
                structure,
                minX,
                maxX,
                maxY,
                cellToStructure
            );


        bool enclosed =
            bottomClosed &&
            topClosed;


        bool wasEnclosed =
            structure.isEnclosedRoom;


        structure.SetEnclosureStatus(
            enclosed
        );


        // =====================================================
        // DEBUG
        // =====================================================

        if (wasEnclosed != enclosed)
        {
            if (enclosed)
            {
                DebugLog(
                    $"ROOM CREATED | " +
                    $"ID: {structure.id} | " +
                    $"Floors: {structure.cells.Count} | " +
                    $"Walls: {structure.totalWallCount} | " +
                    $"Doors: {structure.totalDoorCount}"
                );
            }
            else
            {
                DebugLog(
                    $"ROOM OPENED | " +
                    $"ID: {structure.id} | " +
                    $"Floors: {structure.cells.Count}"
                );
            }
        }
        else
        {
            DebugLog(
                $"ROOM CHECK | " +
                $"ID: {structure.id} | " +
                $"Enclosed: {enclosed} | " +
                $"Floors: {structure.cells.Count} | " +
                $"Walls: {structure.totalWallCount} | " +
                $"Doors: {structure.totalDoorCount}"
            );
        }


        // =====================================================
        // COLLISION
        // =====================================================

        if (enclosed)
        {
            BuildRoomCollision(
                structure,
                cellToStructure
            );
        }
        else
        {
            RemoveRoomCollision(
                structure,
                cellToStructure
            );
        }


        return enclosed;
    }


    // =========================================================
    // CHECK BOTTOM
    // =========================================================

    private bool CheckBottomBoundary(
        BuildingStructure structure,
        int minX,
        int maxX,
        int minY,
        Dictionary<Vector3Int, BuildingStructure> cellToStructure)
    {
        bool closed = true;


        for (
            int x = minX;
            x <= maxX;
            x++)
        {
            Vector3Int floorCell =
                new Vector3Int(
                    x,
                    minY,
                    0
                );


            if (!structure.cells.Contains(floorCell))
                continue;


            if (
                !CheckBoundaryCell(
                    structure,
                    floorCell,
                    cellToStructure
                ))
            {
                closed = false;
            }
        }


        return closed;
    }


    // =========================================================
    // CHECK TOP
    // =========================================================

    private bool CheckTopBoundary(
        BuildingStructure structure,
        int minX,
        int maxX,
        int maxY,
        Dictionary<Vector3Int, BuildingStructure> cellToStructure)
    {
        bool closed = true;


        for (
            int x = minX;
            x <= maxX;
            x++)
        {
            Vector3Int wallCell =
                new Vector3Int(
                    x,
                    maxY + 1,
                    0
                );


            if (
                !CheckBoundaryCell(
                    structure,
                    wallCell,
                    cellToStructure
                ))
            {
                closed = false;
            }
        }


        return closed;
    }


    // =========================================================
    // CHECK BOUNDARY CELL
    // =========================================================

    private bool CheckBoundaryCell(
        BuildingStructure floorStructure,
        Vector3Int boundaryCell,
        Dictionary<Vector3Int, BuildingStructure> cellToStructure)
    {
        if (
            !cellToStructure.TryGetValue(
                boundaryCell,
                out var boundary
            ) ||
            boundary == null ||
            boundary.itemType == null)
        {
            return false;
        }


        TileCategory category =
            boundary.itemType.category;


        if (category == TileCategory.Wall)
        {
            floorStructure.totalWallCount++;

            return true;
        }


        if (category == TileCategory.Door)
        {
            floorStructure.totalDoorCount++;

            return true;
        }


        return false;
    }


    // =========================================================
    // BUILD ROOM COLLISION
    //
    // IMPORTANT:
    //
    // Only OUTER floor tiles receive collision.
    //
    // Interior floor tiles are NOT added.
    //
    // Walls and doors around the outer floor are also added.
    // =========================================================

    public void BuildRoomCollision(
        BuildingStructure structure,
        Dictionary<Vector3Int, BuildingStructure> cellToStructure)
    {
        if (
            structure == null ||
            !structure.isEnclosedRoom ||
            structure.cells == null ||
            structure.cells.Count == 0 ||
            roomCollisionTilemap == null ||
            sourceTilemap == null)
        {
            return;
        }


        int outerFloorCount = 0;
        int boundaryCount = 0;


        // =====================================================
        // FIRST REMOVE OLD COLLISION
        //
        // This prevents old perimeter tiles from remaining
        // after the room changes shape.
        // =====================================================

        RemoveRoomCollision(
            structure,
            cellToStructure,
            false
        );


        // =====================================================
        // BUILD NEW COLLISION
        // =====================================================

        foreach (var floorCell in structure.cells)
        {
            bool isOuterFloor = false;


            // =================================================
            // CHECK WHETHER THIS FLOOR IS ON THE OUTER EDGE
            // =================================================

            foreach (var dir in AdjacentDirections)
            {
                Vector3Int neighborCell =
                    floorCell + dir;


                if (!structure.cells.Contains(neighborCell))
                {
                    isOuterFloor = true;
                    break;
                }
            }


            // Interior floor.
            if (!isOuterFloor)
                continue;


            // =================================================
            // OUTER FLOOR COLLISION
            // =================================================

            TileBase floorTile =
                sourceTilemap.GetTile(
                    floorCell
                );


            if (floorTile != null)
            {
                roomCollisionTilemap.SetTile(
                    floorCell,
                    floorTile
                );

                outerFloorCount++;
            }


            // =================================================
            // WALL / DOOR COLLISION
            // =================================================

            foreach (var dir in AdjacentDirections)
            {
                Vector3Int boundaryCell =
                    floorCell + dir;


                if (
                    !cellToStructure.TryGetValue(
                        boundaryCell,
                        out var boundaryStructure
                    ) ||
                    boundaryStructure == null ||
                    boundaryStructure.itemType == null)
                {
                    continue;
                }


                TileCategory category =
                    boundaryStructure.itemType.category;


                if (
                    category != TileCategory.Wall &&
                    category != TileCategory.Door)
                {
                    continue;
                }


                TileBase boundaryTile =
                    sourceTilemap.GetTile(
                        boundaryCell
                    );


                if (boundaryTile != null)
                {
                    roomCollisionTilemap.SetTile(
                        boundaryCell,
                        boundaryTile
                    );

                    boundaryCount++;
                }
            }
        }


        roomCollisionTilemap.RefreshAllTiles();


        DebugLog(
            $"COLLISION BUILT | " +
            $"Room: {structure.id} | " +
            $"Outer Floors: {outerFloorCount} | " +
            $"Walls/Doors: {boundaryCount} | " +
            $"Interior Floors: {structure.cells.Count - outerFloorCount}"
        );
    }


    // =========================================================
    // REMOVE ROOM COLLISION
    // =========================================================

    public void RemoveRoomCollision(
        BuildingStructure structure,
        Dictionary<Vector3Int, BuildingStructure> cellToStructure)
    {
        RemoveRoomCollision(
            structure,
            cellToStructure,
            true
        );
    }


    // =========================================================
    // REMOVE ROOM COLLISION INTERNAL
    // =========================================================

    private void RemoveRoomCollision(
        BuildingStructure structure,
        Dictionary<Vector3Int, BuildingStructure> cellToStructure,
        bool debug)
    {
        if (
            structure == null ||
            structure.cells == null ||
            roomCollisionTilemap == null)
        {
            return;
        }


        HashSet<Vector3Int> cellsToRemove =
            new HashSet<Vector3Int>();


        // =====================================================
        // FLOOR CELLS
        // =====================================================

        foreach (var floorCell in structure.cells)
        {
            cellsToRemove.Add(
                floorCell
            );


            // =================================================
            // NEIGHBOURING WALL / DOOR CELLS
            // =================================================

            foreach (var dir in AdjacentDirections)
            {
                Vector3Int boundaryCell =
                    floorCell + dir;


                if (
                    !cellToStructure.TryGetValue(
                        boundaryCell,
                        out var boundary
                    ) ||
                    boundary == null ||
                    boundary.itemType == null)
                {
                    continue;
                }


                TileCategory category =
                    boundary.itemType.category;


                if (
                    category == TileCategory.Wall ||
                    category == TileCategory.Door)
                {
                    cellsToRemove.Add(
                        boundaryCell
                    );
                }
            }
        }


        foreach (var cell in cellsToRemove)
        {
            roomCollisionTilemap.SetTile(
                cell,
                null
            );
        }


        roomCollisionTilemap.RefreshAllTiles();


        if (debug)
        {
            DebugLog(
                $"COLLISION REMOVED | " +
                $"Room: {structure.id} | " +
                $"Cells cleared: {cellsToRemove.Count}"
            );
        }
    }


    // =========================================================
    // CLEAR ALL ROOM COLLISION
    // =========================================================

    public void ClearAllRoomCollision()
    {
        if (roomCollisionTilemap == null)
            return;


        roomCollisionTilemap.ClearAllTiles();
        roomCollisionTilemap.RefreshAllTiles();


        DebugLog(
            "ALL ROOM COLLISION CLEARED."
        );
    }
}