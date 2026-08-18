using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// =========================================================
// BUILDING STRUCTURE
// =========================================================

[Serializable]
public class BuildingStructure
{
    public string id = Guid.NewGuid().ToString();

    public buildSO itemType;

    public HashSet<Vector3Int> cells =
        new HashSet<Vector3Int>();

    public bool isEnclosedRoom;

    public int totalWallCount;
    public int totalDoorCount;

    public Action<BuildingStructure, bool>
        OnEnclosureStatusChanged;


    // =====================================================
    // SET ENCLOSURE STATUS
    // =====================================================

    public void SetEnclosureStatus(bool newStatus)
    {
        if (isEnclosedRoom == newStatus)
            return;

        isEnclosedRoom = newStatus;

        if (isEnclosedRoom)
        {
            Debug.Log(
                $"<color=green>" +
                $"[ROOM CREATED]" +
                $"</color> | " +
                $"Structure: {id} | " +
                $"Item: {(itemType != null ? itemType.name : "NULL")} | " +
                $"Floors: {cells.Count} | " +
                $"Walls: {totalWallCount} | " +
                $"Doors: {totalDoorCount}"
            );
        }
        else
        {
            Debug.Log(
                $"<color=red>" +
                $"[ROOM OPENED]" +
                $"</color> | " +
                $"Structure: {id} | " +
                $"Item: {(itemType != null ? itemType.name : "NULL")}"
            );
        }

        OnEnclosureStatusChanged?.Invoke(
            this,
            isEnclosedRoom
        );
    }
}


// =========================================================
// BASE TILE PLACEMENT MANAGER
// =========================================================

public class BaseTilePlacementManager : MonoBehaviour
{
    public static BaseTilePlacementManager Instance;


    // =====================================================
    // STRUCTURES
    // =====================================================

    [Header("Active Structures")]
    public List<BuildingStructure> structures =
        new List<BuildingStructure>();


    // =====================================================
    // CELL LOOKUP
    // =====================================================

    private readonly Dictionary<
        Vector3Int,
        BuildingStructure
    > cellToStructure =
        new Dictionary<
            Vector3Int,
            BuildingStructure
        >();


    // =====================================================
    // DIRECTIONS
    // =====================================================

    private static readonly Vector3Int[] AdjacentDirections =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
    };


    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // =====================================================
    // REGISTER
    // =====================================================

    public void Register(
        buildSO item,
        List<Vector3Int> newCells)
    {
        if (item == null)
        {
            Debug.LogWarning(
                "[ROOM SYSTEM] Register called with NULL item."
            );

            return;
        }

        if (newCells == null || newCells.Count == 0)
        {
            Debug.LogWarning(
                "[ROOM SYSTEM] Register called with no cells."
            );

            return;
        }


        // =================================================
        // FIND NEIGHBOUR STRUCTURES
        // =================================================

        HashSet<BuildingStructure> neighborStructures =
            new HashSet<BuildingStructure>();

        foreach (Vector3Int cell in newCells)
        {
            foreach (Vector3Int direction in AdjacentDirections)
            {
                Vector3Int neighborCell =
                    cell + direction;

                if (!cellToStructure.TryGetValue(
                    neighborCell,
                    out BuildingStructure neighbor))
                {
                    continue;
                }

                if (
                    neighbor != null &&
                    neighbor.itemType == item
                )
                {
                    neighborStructures.Add(neighbor);
                }
            }
        }


        // =================================================
        // CREATE OR MERGE
        // =================================================

        BuildingStructure targetStructure;

        if (neighborStructures.Count == 0)
        {
            targetStructure = new BuildingStructure
            {
                itemType = item
            };

            structures.Add(targetStructure);
        }
        else
        {
            using (
                HashSet<BuildingStructure>.Enumerator
                enumerator =
                neighborStructures.GetEnumerator()
            )
            {
                enumerator.MoveNext();
                targetStructure = enumerator.Current;
            }

            foreach (BuildingStructure other in neighborStructures)
            {
                if (
                    other == null ||
                    other == targetStructure
                )
                {
                    continue;
                }

                foreach (Vector3Int cell in other.cells)
                {
                    targetStructure.cells.Add(cell);
                    cellToStructure[cell] =
                        targetStructure;
                }

                structures.Remove(other);
            }
        }


        // =================================================
        // ADD CELLS
        // =================================================

        foreach (Vector3Int cell in newCells)
        {
            targetStructure.cells.Add(cell);
            cellToStructure[cell] =
                targetStructure;
        }


        // =================================================
        // ROOM VALIDATION
        // =================================================

        if (item.category == TileCategory.Floor)
        {
            ValidateRoomEnclosure(
                targetStructure
            );
        }
        else if (
            item.category == TileCategory.Wall ||
            item.category == TileCategory.Door
        )
        {
            ValidateNearbyFloorStructures(
                newCells
            );
        }


        // =================================================
        // UPDATE WALL VISUALS
        // =================================================

        UpdateWallVisuals(newCells);
    }


    // =====================================================
    // UPDATE WALL VISUALS
    // =====================================================

    private void UpdateWallVisuals(
        List<Vector3Int> changedCells)
    {
        if (
            wallManager.Instance == null ||
            changedCells == null
        )
        {
            return;
        }

        foreach (Vector3Int cell in changedCells)
        {
            wallManager.Instance.UpdateWallsAround(
                new Vector2Int(
                    cell.x,
                    cell.y
                )
            );
        }
    }


    // =====================================================
    // RESTORE
    // =====================================================

    public void RestoreSavedStructures(
        List<BuildingStructureSaveData> savedStructures,
        ItemDatabase database)
    {
        if (database == null)
        {
            Debug.LogError(
                "[ROOM SYSTEM] Cannot restore structures. Database is NULL."
            );

            return;
        }

        structures.Clear();
        cellToStructure.Clear();

        if (
            savedStructures == null ||
            savedStructures.Count == 0
        )
        {
            Debug.Log(
                "[ROOM SYSTEM] No saved structures to restore."
            );

            return;
        }

        int restoredCount = 0;

        foreach (
            BuildingStructureSaveData saved
            in savedStructures
        )
        {
            if (saved == null)
                continue;

            ItemData item =
                database.GetItemByID(
                    saved.itemID
                );

            if (
                item == null ||
                item is not buildSO build
            )
            {
                Debug.LogWarning(
                    "[ROOM SYSTEM] Could not restore structure item ID: " +
                    saved.itemID
                );

                continue;
            }

            BuildingStructure structure =
                new BuildingStructure
                {
                    itemType = build
                };

            if (!string.IsNullOrEmpty(saved.id))
            {
                structure.id = saved.id;
            }

            if (saved.cells != null)
            {
                foreach (Vector3Int cell in saved.cells)
                {
                    structure.cells.Add(cell);
                    cellToStructure[cell] =
                        structure;
                }
            }

            structure.isEnclosedRoom =
                saved.isEnclosedRoom;

            structure.totalWallCount =
                saved.totalWallCount;

            structure.totalDoorCount =
                saved.totalDoorCount;

            structures.Add(structure);

            restoredCount++;
        }


        // =================================================
        // REVALIDATE FLOORS
        // =================================================

        foreach (BuildingStructure structure in structures)
        {
            if (
                structure == null ||
                structure.itemType == null ||
                structure.cells == null ||
                structure.cells.Count == 0
            )
            {
                continue;
            }

            if (
                structure.itemType.category ==
                TileCategory.Floor
            )
            {
                ValidateRoomEnclosure(
                    structure
                );
            }
        }

        Debug.Log(
            $"<color=green>" +
            $"[ROOM SYSTEM] RESTORED STRUCTURES: " +
            $"{restoredCount}" +
            $"</color>"
        );
    }


    // =====================================================
    // VALIDATE NEARBY FLOORS
    // =====================================================

    private void ValidateNearbyFloorStructures(
        List<Vector3Int> changedCells)
    {
        if (
            changedCells == null ||
            changedCells.Count == 0
        )
        {
            return;
        }

        HashSet<BuildingStructure> floorStructures =
            new HashSet<BuildingStructure>();

        foreach (Vector3Int cell in changedCells)
        {
            foreach (Vector3Int direction in AdjacentDirections)
            {
                Vector3Int neighborCell =
                    cell + direction;

                if (
                    !cellToStructure.TryGetValue(
                        neighborCell,
                        out BuildingStructure structure
                    )
                )
                {
                    continue;
                }

                if (
                    structure != null &&
                    structure.itemType != null &&
                    structure.itemType.category ==
                    TileCategory.Floor
                )
                {
                    floorStructures.Add(structure);
                }
            }
        }

        foreach (
            BuildingStructure floorStructure
            in floorStructures
        )
        {
            ValidateRoomEnclosure(
                floorStructure
            );
        }
    }


    // =====================================================
    // ROOM ENCLOSURE
    // =====================================================

    public bool ValidateRoomEnclosure(
        BuildingStructure structure)
    {
        if (
            structure == null ||
            structure.cells == null ||
            structure.cells.Count == 0
        )
        {
            return false;
        }

        if (
            structure.itemType == null ||
            structure.itemType.category !=
            TileCategory.Floor
        )
        {
            return false;
        }

        structure.totalWallCount = 0;
        structure.totalDoorCount = 0;

        int minX = int.MaxValue;
        int maxX = int.MinValue;

        int minY = int.MaxValue;
        int maxY = int.MinValue;


        // =================================================
        // CALCULATE BOUNDS
        // =================================================

        foreach (Vector3Int cell in structure.cells)
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


        // =================================================
        // CHECK BOTTOM
        // =================================================

        bool bottomClosed =
            CheckBottomBoundary(
                structure,
                minX,
                maxX,
                minY
            );


        // =================================================
        // CHECK TOP
        // =================================================

        bool topClosed =
            CheckTopBoundary(
                structure,
                minX,
                maxX,
                maxY
            );


        bool enclosed =
            bottomClosed &&
            topClosed;

        structure.SetEnclosureStatus(
            enclosed
        );

        return enclosed;
    }


    // =====================================================
    // BOTTOM
    // =====================================================

    private bool CheckBottomBoundary(
        BuildingStructure structure,
        int minX,
        int maxX,
        int minY)
    {
        bool closed = true;

        for (int x = minX; x <= maxX; x++)
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
                    floorCell
                )
            )
            {
                closed = false;
            }
        }

        return closed;
    }


    // =====================================================
    // TOP
    // =====================================================

    private bool CheckTopBoundary(
        BuildingStructure structure,
        int minX,
        int maxX,
        int maxY)
    {
        bool closed = true;

        for (int x = minX; x <= maxX; x++)
        {
            // IMPORTANT:
            // The upper wall is ONE TILE ABOVE
            // the top floor row.

            Vector3Int wallCell =
                new Vector3Int(
                    x,
                    maxY + 1,
                    0
                );

            if (
                !CheckBoundaryCell(
                    structure,
                    wallCell
                )
            )
            {
                closed = false;
            }
        }

        return closed;
    }


    // =====================================================
    // CHECK BOUNDARY
    // =====================================================

    private bool CheckBoundaryCell(
        BuildingStructure floorStructure,
        Vector3Int boundaryCell)
    {
        if (
            !cellToStructure.TryGetValue(
                boundaryCell,
                out BuildingStructure boundary
            )
        )
        {
            return false;
        }

        if (
            boundary == null ||
            boundary.itemType == null
        )
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


    // =====================================================
    // REMOVE PLACEMENT
    // =====================================================

    public bool RemovePlacement(
        Tilemap tilemap,
        Vector3Int cell)
    {
        if (
            tilemap == null ||
            !cellToStructure.TryGetValue(
                cell,
                out BuildingStructure originalStructure
            )
        )
        {
            return false;
        }

        HashSet<BuildingStructure> affectedRooms =
            FindNearbyFloorStructures(cell);


        // =================================================
        // REMOVE TILE
        // =================================================

        tilemap.SetTile(
            cell,
            null
        );

        cellToStructure.Remove(cell);

        originalStructure.cells.Remove(cell);


        // =================================================
        // RETURN ITEM
        // =================================================

        if (
            originalStructure.itemType != null &&
            InventoryManager.Instance != null
        )
        {
            InventoryManager.Instance.AddItem(
                originalStructure.itemType,
                1
            );
        }


        // =================================================
        // UPDATE WALL VISUALS
        // =================================================

        UpdateWallVisuals(
            new List<Vector3Int>
            {
                cell
            }
        );


        // =================================================
        // STRUCTURE EMPTY
        // =================================================

        if (originalStructure.cells.Count == 0)
        {
            structures.Remove(
                originalStructure
            );

            foreach (
                BuildingStructure room
                in affectedRooms
            )
            {
                if (
                    room != null &&
                    room.cells.Count > 0
                )
                {
                    ValidateRoomEnclosure(room);
                }
            }

            tilemap.RefreshAllTiles();

            SaveWorldAfterChange();

            return true;
        }


        // =================================================
        // SPLIT STRUCTURE
        // =================================================

        RebuildAndSplitStructure(
            originalStructure
        );


        // =================================================
        // REVALIDATE AFFECTED ROOMS
        // =================================================

        foreach (
            BuildingStructure room
            in affectedRooms
        )
        {
            if (
                room != null &&
                room.cells.Count > 0
            )
            {
                ValidateRoomEnclosure(room);
            }
        }

        tilemap.RefreshAllTiles();

        SaveWorldAfterChange();

        return true;
    }


    // =====================================================
    // SAVE
    // =====================================================

    private void SaveWorldAfterChange()
    {
        if (
            BuildingSaveManager.Instance != null
        )
        {
            BuildingSaveManager.Instance
                .SaveAfterChange();
        }
    }


    // =====================================================
    // FIND NEARBY FLOORS
    // =====================================================

    private HashSet<BuildingStructure>
        FindNearbyFloorStructures(
            Vector3Int cell)
    {
        HashSet<BuildingStructure> result =
            new HashSet<BuildingStructure>();

        foreach (Vector3Int direction in AdjacentDirections)
        {
            Vector3Int neighborCell =
                cell + direction;

            if (
                !cellToStructure.TryGetValue(
                    neighborCell,
                    out BuildingStructure structure
                )
            )
            {
                continue;
            }

            if (
                structure != null &&
                structure.itemType != null &&
                structure.itemType.category ==
                TileCategory.Floor
            )
            {
                result.Add(structure);
            }
        }

        return result;
    }


    // =====================================================
    // SPLIT
    // =====================================================

    private void RebuildAndSplitStructure(
        BuildingStructure originalStructure)
    {
        HashSet<Vector3Int> unvisited =
            new HashSet<Vector3Int>(
                originalStructure.cells
            );

        structures.Remove(
            originalStructure
        );


        while (unvisited.Count > 0)
        {
            BuildingStructure newStructure =
                new BuildingStructure
                {
                    itemType =
                        originalStructure.itemType
                };

            Queue<Vector3Int> queue =
                new Queue<Vector3Int>();

            Vector3Int start = default;

            foreach (Vector3Int first in unvisited)
            {
                start = first;
                break;
            }

            queue.Enqueue(start);
            unvisited.Remove(start);


            // =================================================
            // FLOOD FILL
            // =================================================

            while (queue.Count > 0)
            {
                Vector3Int current =
                    queue.Dequeue();

                newStructure.cells.Add(
                    current
                );

                cellToStructure[current] =
                    newStructure;


                foreach (
                    Vector3Int direction
                    in AdjacentDirections
                )
                {
                    Vector3Int neighbor =
                        current + direction;

                    if (!unvisited.Contains(neighbor))
                        continue;

                    unvisited.Remove(neighbor);

                    queue.Enqueue(neighbor);
                }
            }


            structures.Add(
                newStructure
            );


            // =================================================
            // REVALIDATE
            // =================================================

            ValidateRoomEnclosure(
                newStructure
            );
        }
    }


    // =====================================================
    // DEBUG
    // =====================================================

    [ContextMenu("Debug Room Structures")]
    public void DebugRoomStructures()
    {
        Debug.Log(
            "<color=cyan>" +
            "========== ROOM STRUCTURES ==========" +
            "</color>"
        );

        foreach (
            BuildingStructure structure
            in structures
        )
        {
            if (
                structure == null ||
                structure.itemType == null
            )
            {
                continue;
            }

            Debug.Log(
                $"[STRUCTURE] | " +
                $"ID: {structure.id} | " +
                $"Item: {structure.itemType.name} | " +
                $"Category: {structure.itemType.category} | " +
                $"Cells: {structure.cells.Count} | " +
                $"Room: {structure.isEnclosedRoom} | " +
                $"Walls: {structure.totalWallCount} | " +
                $"Doors: {structure.totalDoorCount}"
            );

            foreach (
                Vector3Int cell
                in structure.cells
            )
            {
                Debug.Log(
                    $"    Cell: {cell}"
                );
            }
        }

        Debug.Log(
            "<color=cyan>" +
            "======================================" +
            "</color>"
        );
    }
}
