using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBorderCollider : MonoBehaviour
{
    [Header("Room Grid Position")]
    public Vector2Int roomPosition;

    [Header("Room Size In Tiles")]
    public Vector2Int roomSize = new Vector2Int(5, 5);

    [Header("Openings & Doors")]
    [Tooltip("First bottom opening tile index. -1 = no opening.")]
    public int bottomOpening1 = -1;

    [Tooltip("Second bottom opening tile index. -1 = no opening.")]
    public int bottomOpening2 = -1;

    [Tooltip("Primary door tile asset.")]
    public TileBase doorTile;

    [Tooltip("Secondary door tile asset.")]
    public TileBase doorTile2;

    [Tooltip("Or add any number of additional door tiles here.")]
    public TileBase[] additionalDoorTiles;

    private Tilemap tilemap;

    private List<GameObject> topWalls = new();
    private List<GameObject> bottomWalls = new();
    private List<GameObject> leftWalls = new();
    private List<GameObject> rightWalls = new();

    private bool generated;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError($"[Border] No Tilemap found on {name}");
            return;
        }

        GenerateBorderColliders();
        generated = true;
    }

    public bool IsDoorTile(TileBase tile)
    {
        if (tile == null) return false;

        if (doorTile != null && tile == doorTile)
            return true;

        if (doorTile2 != null && tile == doorTile2)
            return true;

        if (additionalDoorTiles != null)
        {
            foreach (var door in additionalDoorTiles)
            {
                if (door != null && tile == door)
                    return true;
            }
        }

        return false;
    }

    private bool IsAdjacentToAnyDoor(Vector3Int tilePos)
    {
        Vector3Int[] directions =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (var dir in directions)
        {
            TileBase neighbor = tilemap.GetTile(tilePos + dir);

            if (IsDoorTile(neighbor))
                return true;
        }

        return false;
    }

    // ===================================================
    // CREATE WALL COLLIDERS
    // ===================================================

    private void GenerateBorderColliders()
    {
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos))
                continue;

            // Skip if the current tile is a door tile
            if (IsDoorTile(tilemap.GetTile(pos)))
                continue;

            // Skip if this tile touches a door tile
            // on any of its 4 sides
            if (IsAdjacentToAnyDoor(pos))
                continue;

            CheckSide(pos, Vector3Int.up);
            CheckSide(pos, Vector3Int.down);
            CheckSide(pos, Vector3Int.left);
            CheckSide(pos, Vector3Int.right);
        }
    }

    // ===================================================
    // CHECK SIDE
    // ===================================================

    private void CheckSide(Vector3Int tile, Vector3Int direction)
    {
        Vector3Int neighborPos = tile + direction;
        TileBase neighborTile = tilemap.GetTile(neighborPos);

        // Skip if another solid wall tile exists
        // adjacent to this side
        if (neighborTile != null && !IsDoorTile(neighborTile))
            return;

        // Skip first bottom opening
        if (direction == Vector3Int.down && tile.x == bottomOpening1)
            return;

        // Skip second bottom opening
        if (direction == Vector3Int.down && tile.x == bottomOpening2)
            return;

        // ===================================================
        // CREATE WALL
        // ===================================================

        GameObject wall = new GameObject(
            $"Border_{direction}_{tile}"
        );

        wall.transform.SetParent(transform);

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.isTrigger = false;

        Vector3 center = tilemap.GetCellCenterWorld(tile);
        Vector3 size = tilemap.cellSize;

        if (direction == Vector3Int.up)
        {
            wall.transform.position =
                center + new Vector3(0, size.y / 2f, 0);

            col.size = new Vector2(size.x, 0.05f);

            topWalls.Add(wall);
        }
        else if (direction == Vector3Int.down)
        {
            wall.transform.position =
                center - new Vector3(0, size.y / 2f, 0);

            col.size = new Vector2(size.x, 0.05f);

            bottomWalls.Add(wall);
        }
        else if (direction == Vector3Int.left)
        {
            wall.transform.position =
                center - new Vector3(size.x / 2f, 0, 0);

            col.size = new Vector2(0.05f, size.y);

            leftWalls.Add(wall);
        }
        else if (direction == Vector3Int.right)
        {
            wall.transform.position =
                center + new Vector3(size.x / 2f, 0, 0);

            col.size = new Vector2(0.05f, size.y);

            rightWalls.Add(wall);
        }
    }
}