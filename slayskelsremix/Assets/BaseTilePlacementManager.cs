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

        OnEnclosureStatusChanged?.Invoke(
            this,
            newStatus
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
        BuildingStructure>
        cellToStructure =
        new Dictionary<
            Vector3Int,
            BuildingStructure>();


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
        if (item == null ||
            newCells == null ||
            newCells.Count == 0)
        {
            return;
        }


        // =================================================
        // FIND NEIGHBOUR STRUCTURES
        // =================================================

        BuildingStructure targetStructure = null;

        HashSet<BuildingStructure> neighborStructures = null;


        foreach (Vector3Int cell in newCells)
        {
            for (int i = 0; i < AdjacentDirections.Length; i++)
            {
                Vector3Int neighborCell =
                    cell + AdjacentDirections[i];


                if (!cellToStructure.TryGetValue(
                        neighborCell,
                        out BuildingStructure neighbor))
                {
                    continue;
                }


                if (neighbor == null ||
                    neighbor.itemType != item)
                {
                    continue;
                }


                if (neighborStructures == null)
                {
                    neighborStructures =
                        new HashSet<BuildingStructure>();
                }


                neighborStructures.Add(neighbor);
            }
        }


        // =================================================
        // CREATE NEW STRUCTURE
        // =================================================

        if (neighborStructures == null ||
            neighborStructures.Count == 0)
        {
            targetStructure =
                new BuildingStructure
                {
                    itemType = item
                };

            structures.Add(targetStructure);
        }

        // =================================================
        // MERGE STRUCTURES
        // =================================================

        else
        {
            foreach (BuildingStructure structure
                     in neighborStructures)
            {
                if (structure == null)
                    continue;


                if (targetStructure == null)
                {
                    targetStructure = structure;
                    continue;
                }


                if (structure == targetStructure)
                    continue;


                foreach (Vector3Int cell
                         in structure.cells)
                {
                    targetStructure.cells.Add(cell);
                    cellToStructure[cell] =
                        targetStructure;
                }


                structures.Remove(structure);
            }
        }


        // =================================================
        // ADD NEW CELLS
        // =================================================

        for (int i = 0; i < newCells.Count; i++)
        {
            Vector3Int cell = newCells[i];

            targetStructure.cells.Add(cell);
            cellToStructure[cell] =
                targetStructure;
        }


        // =================================================
        // VALIDATE ROOM
        // =================================================

        if (item.category == TileCategory.Floor)
        {
            ValidateRoomEnclosure(
                targetStructure
            );
        }
        else if (
            item.category == TileCategory.Wall ||
            item.category == TileCategory.Door)
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
        if (changedCells == null ||
            changedCells.Count == 0 ||
            wallManager.Instance == null)
        {
            return;
        }


        for (int i = 0; i < changedCells.Count; i++)
        {
            Vector3Int cell = changedCells[i];

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
            return;


        structures.Clear();
        cellToStructure.Clear();


        if (savedStructures == null ||
            savedStructures.Count == 0)
        {
            return;
        }


        for (int i = 0; i < savedStructures.Count; i++)
        {
            BuildingStructureSaveData saved =
                savedStructures[i];


            if (saved == null)
                continue;


            ItemData item =
                database.GetItemByID(
                    saved.itemID
                );


            if (item == null ||
                item is not buildSO build)
            {
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
                for (int c = 0; c < saved.cells.Count; c++)
                {
                    Vector3Int cell =
                        saved.cells[c];


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
        }


        // =================================================
        // REVALIDATE FLOORS
        // =================================================

        for (int i = 0; i < structures.Count; i++)
        {
            BuildingStructure structure =
                structures[i];


            if (structure == null ||
                structure.itemType == null ||
                structure.cells == null ||
                structure.cells.Count == 0)
            {
                continue;
            }


            if (structure.itemType.category ==
                TileCategory.Floor)
            {
                ValidateRoomEnclosure(
                    structure
                );
            }
        }
    }


    // =====================================================
    // VALIDATE NEARBY FLOOR STRUCTURES
    // =====================================================

    private void ValidateNearbyFloorStructures(
        List<Vector3Int> changedCells)
    {
        if (changedCells == null ||
            changedCells.Count == 0)
        {
            return;
        }


        HashSet<BuildingStructure> floorStructures =
            new HashSet<BuildingStructure>();


        for (int i = 0; i < changedCells.Count; i++)
        {
            Vector3Int cell =
                changedCells[i];


            for (int d = 0;
                 d < AdjacentDirections.Length;
                 d++)
            {
                Vector3Int neighborCell =
                    cell + AdjacentDirections[d];


                if (!cellToStructure.TryGetValue(
                        neighborCell,
                        out BuildingStructure structure))
                {
                    continue;
                }


                if (structure == null ||
                    structure.itemType == null ||
                    structure.itemType.category !=
                    TileCategory.Floor)
                {
                    continue;
                }


                floorStructures.Add(structure);
            }
        }


        foreach (BuildingStructure floorStructure
                 in floorStructures)
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
        if (structure == null ||
            structure.cells == null ||
            structure.cells.Count == 0)
        {
            return false;
        }


        if (structure.itemType == null ||
            structure.itemType.category !=
            TileCategory.Floor)
        {
            return false;
        }


        structure.totalWallCount = 0;
        structure.totalDoorCount = 0;


        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;


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


        bool bottomClosed =
            CheckBottomBoundary(
                structure,
                minX,
                maxX,
                minY
            );


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


            if (!CheckBoundaryCell(
                    structure,
                    floorCell))
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // TOP
    //
    // CHECKS THE CELL DIRECTLY ABOVE THE FLOOR.
    //
    // FLOOR:
    //
    // F F F F
    //
    // ABOVE:
    //
    // W W W W
    //
    // The checked position is:
    //
    // floorCell + Vector3Int.up
    // =====================================================

    private bool CheckTopBoundary(
        BuildingStructure structure,
        int minX,
        int maxX,
        int maxY)
    {
        for (int x = minX; x <= maxX; x++)
        {
            Vector3Int floorCell =
                new Vector3Int(
                    x,
                    maxY,
                    0
                );


            if (!structure.cells.Contains(floorCell))
                continue;


            Vector3Int cellAbove =
                floorCell + Vector3Int.up;


            if (!CheckBoundaryCell(
                    structure,
                    cellAbove))
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // CHECK BOUNDARY CELL
    // =====================================================

    private bool CheckBoundaryCell(
        BuildingStructure floorStructure,
        Vector3Int boundaryCell)
    {
        if (!cellToStructure.TryGetValue(
                boundaryCell,
                out BuildingStructure boundary))
        {
            return false;
        }


        if (boundary == null ||
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


    // =====================================================
    // REMOVE PLACEMENT
    // =====================================================

    public bool RemovePlacement(
        Tilemap tilemap,
        Vector3Int cell)
    {
        if (tilemap == null)
            return false;


        if (!cellToStructure.TryGetValue(
                cell,
                out BuildingStructure originalStructure))
        {
            return false;
        }


        HashSet<BuildingStructure> affectedRooms =
            FindNearbyFloorStructures(cell);


        tilemap.SetTile(
            cell,
            null
        );


        cellToStructure.Remove(cell);


        originalStructure.cells.Remove(cell);


        if (
            originalStructure.itemType != null &&
            InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(
                originalStructure.itemType,
                1
            );
        }


        UpdateWallVisuals(
            new List<Vector3Int>
            {
                cell
            }
        );


        // =================================================
        // STRUCTURE COMPLETELY REMOVED
        // =================================================

        if (originalStructure.cells.Count == 0)
        {
            structures.Remove(
                originalStructure
            );


            foreach (
                BuildingStructure room
                in affectedRooms)
            {
                if (
                    room != null &&
                    room.cells.Count > 0)
                {
                    ValidateRoomEnclosure(
                        room
                    );
                }
            }


            tilemap.RefreshAllTiles();

            SaveWorldAfterChange();

            return true;
        }


        // =================================================
        // STRUCTURE MAY HAVE SPLIT
        // =================================================

        RebuildAndSplitStructure(
            originalStructure
        );


        foreach (
            BuildingStructure room
            in affectedRooms)
        {
            if (
                room != null &&
                room.cells.Count > 0)
            {
                ValidateRoomEnclosure(
                    room
                );
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
        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.SaveAfterChange();
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


        for (int i = 0;
             i < AdjacentDirections.Length;
             i++)
        {
            Vector3Int neighborCell =
                cell + AdjacentDirections[i];


            if (!cellToStructure.TryGetValue(
                    neighborCell,
                    out BuildingStructure structure))
            {
                continue;
            }


            if (
                structure != null &&
                structure.itemType != null &&
                structure.itemType.category ==
                TileCategory.Floor)
            {
                result.Add(structure);
            }
        }


        return result;
    }


    // =====================================================
    // SPLIT STRUCTURE
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


            Vector3Int start =
                default;


            foreach (Vector3Int cell in unvisited)
            {
                start = cell;
                break;
            }


            queue.Enqueue(start);
            unvisited.Remove(start);


            while (queue.Count > 0)
            {
                Vector3Int current =
                    queue.Dequeue();


                newStructure.cells.Add(
                    current
                );


                cellToStructure[current] =
                    newStructure;


                for (int d = 0;
                     d < AdjacentDirections.Length;
                     d++)
                {
                    Vector3Int neighbor =
                        current +
                        AdjacentDirections[d];


                    if (!unvisited.Contains(neighbor))
                        continue;


                    unvisited.Remove(neighbor);

                    queue.Enqueue(neighbor);
                }
            }


            structures.Add(
                newStructure
            );


            ValidateRoomEnclosure(
                newStructure
            );
        }
    }


    // =====================================================
    // DEBUG
    // =====================================================
    //
    // Removed completely.
    //
    // No Debug.Log
    // No Debug.LogWarning
    // No Debug.LogError
    //
    // =====================================================
}