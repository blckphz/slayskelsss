using System.Collections.Generic;
using UnityEngine;

// =========================================================
// WALL MANAGER
// =========================================================

public class wallManager : MonoBehaviour
{
    public static wallManager Instance { get; private set; }

    // =========================================================
    // WALL LOOKUP
    // =========================================================

    private readonly Dictionary<Vector2Int, WallBehav> walls = new Dictionary<Vector2Int, WallBehav>();

    private static readonly Vector2Int[] neighborOffsets = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
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
    // REGISTER WALL
    // =========================================================

    public void RegisterWall(WallBehav wall)
    {
        if (wall == null) return;
        if (!wall.HasGridPosition()) return;

        Vector2Int position = wall.GetGridPosition();

        walls[position] = wall;

        UpdateWall(position);
        UpdateNeighbors(position);
    }

    // =========================================================
    // UNREGISTER WALL
    // =========================================================

    public void UnregisterWall(WallBehav wall)
    {
        if (wall == null) return;

        Vector2Int position = wall.GetGridPosition();

        if (walls.TryGetValue(position, out WallBehav existingWall) && existingWall == wall)
        {
            walls.Remove(position);
        }

        UpdateNeighbors(position);
    }

    // =========================================================
    // HAS WALL
    // =========================================================

    public bool HasWall(Vector2Int position)
    {
        if (walls.TryGetValue(position, out WallBehav wall))
        {
            if (wall != null) return true;
            walls.Remove(position);
        }

        return false;
    }

    // =========================================================
    // GET WALL
    // =========================================================

    public WallBehav GetWall(Vector2Int position)
    {
        if (walls.TryGetValue(position, out WallBehav wall))
        {
            if (wall != null) return wall;
            walls.Remove(position);
        }

        return null;
    }

    // =========================================================
    // UPDATE WALL
    // =========================================================

    public void UpdateWall(Vector2Int position)
    {
        WallBehav wall = GetWall(position);
        if (wall != null)
        {
            wall.UpdateShape();
        }
    }

    // =========================================================
    // UPDATE NEIGHBOURS
    // =========================================================

    private void UpdateNeighbors(Vector2Int position)
    {
        for (int i = 0; i < neighborOffsets.Length; i++)
        {
            UpdateWall(position + neighborOffsets[i]);
        }
    }

    // =========================================================
    // UPDATE WALLS AROUND CELL
    // =========================================================

    public void UpdateWallsAround(Vector2Int position)
    {
        UpdateWall(position);
        UpdateNeighbors(position);
    }

    // =========================================================
    // FORCE UPDATE ALL
    // =========================================================

    public void UpdateAllWalls()
    {
        List<Vector2Int> positions = new List<Vector2Int>(walls.Keys);

        for (int i = 0; i < positions.Count; i++)
        {
            UpdateWall(positions[i]);
        }
    }

    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearAllWalls()
    {
        walls.Clear();
    }
}