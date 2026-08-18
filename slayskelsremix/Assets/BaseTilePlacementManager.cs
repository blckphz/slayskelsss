using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class BuildingStructure
{
    public string id = Guid.NewGuid().ToString();
    public buildSO itemType;
    public HashSet<Vector3Int> cells = new HashSet<Vector3Int>();
    public bool isEnclosedRoom;
    public int totalWallCount;
    public int totalDoorCount;

    public Action<BuildingStructure, bool> OnEnclosureStatusChanged;

    public void SetEnclosureStatus(bool newStatus)
    {
        if (isEnclosedRoom == newStatus) return;
        isEnclosedRoom = newStatus;

        if (isEnclosedRoom)
        {
            Debug.Log($"[ROOM CREATED] Structure: {id} | Item: {(itemType != null ? itemType.name : "NULL")} | Floors: {cells.Count} | Walls: {totalWallCount} | Doors: {totalDoorCount}");
        }

        OnEnclosureStatusChanged?.Invoke(this, isEnclosedRoom);
    }
}

public class BaseTilePlacementManager : MonoBehaviour
{
    public static BaseTilePlacementManager Instance;

    [Header("Active Structures")]
    public List<BuildingStructure> structures = new List<BuildingStructure>();

    private readonly Dictionary<Vector3Int, BuildingStructure> cellToStructure = new Dictionary<Vector3Int, BuildingStructure>();

    private static readonly Vector3Int[] AdjacentDirections =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(buildSO item, List<Vector3Int> newCells)
    {
        if (item == null || newCells == null || newCells.Count == 0) return;

        HashSet<BuildingStructure> neighborStructures = new HashSet<BuildingStructure>();

        foreach (var cell in newCells)
        {
            foreach (var dir in AdjacentDirections)
            {
                if (cellToStructure.TryGetValue(cell + dir, out var neighbor) && neighbor != null && neighbor.itemType == item)
                {
                    neighborStructures.Add(neighbor);
                }
            }
        }

        BuildingStructure targetStructure;

        if (neighborStructures.Count == 0)
        {
            targetStructure = new BuildingStructure { itemType = item };
            structures.Add(targetStructure);
        }
        else
        {
            var enumerator = neighborStructures.GetEnumerator();
            enumerator.MoveNext();
            targetStructure = enumerator.Current;

            foreach (var other in neighborStructures)
            {
                if (other == null || other == targetStructure) continue;

                foreach (var cell in other.cells)
                {
                    targetStructure.cells.Add(cell);
                    cellToStructure[cell] = targetStructure;
                }
                structures.Remove(other);
            }
        }

        foreach (var cell in newCells)
        {
            targetStructure.cells.Add(cell);
            cellToStructure[cell] = targetStructure;
        }

        if (item.category == TileCategory.Floor)
        {
            RoomCompletionCheckerManager.Instance?.ValidateRoomEnclosure(targetStructure, cellToStructure);
        }
        else if (item.category == TileCategory.Wall || item.category == TileCategory.Door)
        {
            RoomCompletionCheckerManager.Instance?.ValidateNearbyFloorStructures(newCells, cellToStructure);
        }

        UpdateWallVisuals(newCells);
    }

    private void UpdateWallVisuals(List<Vector3Int> changedCells)
    {
        if (wallManager.Instance == null || changedCells == null) return;

        foreach (var cell in changedCells)
        {
            wallManager.Instance.UpdateWallsAround(new Vector2Int(cell.x, cell.y));
        }
    }

    public void RestoreSavedStructures(List<BuildingStructureSaveData> savedStructures, ItemDatabase database)
    {
        if (database == null) return;

        structures.Clear();
        cellToStructure.Clear();
        RoomCompletionCheckerManager.Instance?.ClearAllRoomCollision();

        if (savedStructures == null || savedStructures.Count == 0) return;

        foreach (var saved in savedStructures)
        {
            if (saved == null) continue;

            if (database.GetItemByID(saved.itemID) is not buildSO build) continue;

            var structure = new BuildingStructure
            {
                itemType = build,
                isEnclosedRoom = saved.isEnclosedRoom,
                totalWallCount = saved.totalWallCount,
                totalDoorCount = saved.totalDoorCount
            };

            if (!string.IsNullOrEmpty(saved.id)) structure.id = saved.id;

            if (saved.cells != null)
            {
                foreach (var cell in saved.cells)
                {
                    structure.cells.Add(cell);
                    cellToStructure[cell] = structure;
                }
            }

            structures.Add(structure);
        }

        foreach (var structure in structures)
        {
            if (structure?.itemType == null || structure.cells.Count == 0) continue;

            if (structure.itemType.category == TileCategory.Floor)
            {
                RoomCompletionCheckerManager.Instance?.ValidateRoomEnclosure(structure, cellToStructure);
            }
        }

        foreach (var structure in structures)
        {
            if (structure?.itemType == null) continue;

            if (structure.itemType.category == TileCategory.Floor && structure.isEnclosedRoom)
            {
                RoomCompletionCheckerManager.Instance?.BuildRoomCollision(structure, cellToStructure);
            }
        }
    }

    public bool RemovePlacement(Tilemap tilemap, Vector3Int cell)
    {
        if (tilemap == null || !cellToStructure.TryGetValue(cell, out var originalStructure)) return false;

        var affectedRooms = FindNearbyFloorStructures(cell);

        if (originalStructure?.itemType?.category == TileCategory.Floor && originalStructure.isEnclosedRoom)
        {
            RoomCompletionCheckerManager.Instance?.RemoveRoomCollision(originalStructure, cellToStructure);
        }

        tilemap.SetTile(cell, null);
        cellToStructure.Remove(cell);
        originalStructure.cells.Remove(cell);

        if (originalStructure.itemType != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(originalStructure.itemType, 1);
        }

        if (RoomCompletionCheckerManager.Instance?.roomCollisionTilemap != null)
            RoomCompletionCheckerManager.Instance.roomCollisionTilemap.SetTile(cell, null);

        UpdateWallVisuals(new List<Vector3Int> { cell });

        if (originalStructure.cells.Count == 0)
        {
            structures.Remove(originalStructure);
            CleanupAfterRemoval(affectedRooms, tilemap);
            return true;
        }

        RebuildAndSplitStructure(originalStructure);
        CleanupAfterRemoval(affectedRooms, tilemap);
        return true;
    }

    private void CleanupAfterRemoval(HashSet<BuildingStructure> affectedRooms, Tilemap tilemap)
    {
        foreach (var room in affectedRooms)
        {
            if (room != null && room.cells.Count > 0)
            {
                RoomCompletionCheckerManager.Instance?.ValidateRoomEnclosure(room, cellToStructure);
            }
        }

        RoomCompletionCheckerManager.Instance?.roomCollisionTilemap?.RefreshAllTiles();
        tilemap.RefreshAllTiles();
        SaveWorldAfterChange();
    }

    private void SaveWorldAfterChange()
    {
        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.SaveAfterChange();
        }
    }

    private HashSet<BuildingStructure> FindNearbyFloorStructures(Vector3Int cell)
    {
        var result = new HashSet<BuildingStructure>();
        foreach (var dir in AdjacentDirections)
        {
            if (cellToStructure.TryGetValue(cell + dir, out var structure) && structure?.itemType?.category == TileCategory.Floor)
            {
                result.Add(structure);
            }
        }
        return result;
    }

    private void RebuildAndSplitStructure(BuildingStructure originalStructure)
    {
        if (originalStructure?.isEnclosedRoom == true)
        {
            RoomCompletionCheckerManager.Instance?.RemoveRoomCollision(originalStructure, cellToStructure);
        }

        var unvisited = new HashSet<Vector3Int>(originalStructure.cells);
        structures.Remove(originalStructure);

        while (unvisited.Count > 0)
        {
            var newStructure = new BuildingStructure { itemType = originalStructure.itemType };
            var queue = new Queue<Vector3Int>();

            Vector3Int start = default;
            foreach (var first in unvisited)
            {
                start = first;
                break;
            }

            queue.Enqueue(start);
            unvisited.Remove(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                newStructure.cells.Add(current);
                cellToStructure[current] = newStructure;

                foreach (var dir in AdjacentDirections)
                {
                    var neighbor = current + dir;
                    if (!unvisited.Contains(neighbor)) continue;

                    unvisited.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            structures.Add(newStructure);
            RoomCompletionCheckerManager.Instance?.ValidateRoomEnclosure(newStructure, cellToStructure);
        }
    }
}