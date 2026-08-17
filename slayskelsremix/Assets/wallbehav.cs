using UnityEngine;
using UnityEngine.Tilemaps;

public class WallBehav : MonoBehaviour
{
    [Header("Wall Tilemap")]
    [SerializeField] private Tilemap wallTilemap;

    [Header("Wall Tiles")]
    [SerializeField] private TileBase[] wallTiles;

    /*
        TILE LAYOUT

        1   2   3
        4   5   6
        7   8   9

        C# ARRAY

        wallTiles[0] = 1
        wallTiles[1] = 2
        wallTiles[2] = 3

        wallTiles[3] = 4
        wallTiles[4] = 5
        wallTiles[5] = 6

        wallTiles[6] = 7
        wallTiles[7] = 8
        wallTiles[8] = 9
    */

    private Vector2Int gridPos;

    private void Awake()
    {
        if (wallTilemap == null)
        {
            wallTilemap = GetComponentInChildren<Tilemap>();
        }

        gridPos = Vector2Int.RoundToInt(transform.position);
    }

    private void Start()
    {
        if (wallManager.Instance != null)
        {
            wallManager.Instance.RegisterWall(this);
        }
        else
        {
            Debug.LogError("WallBehav: No wallManager found in scene!");
        }
    }

    public Vector2Int GetGridPosition()
    {
        return gridPos;
    }

    public void UpdateShape()
    {
        if (wallManager.Instance == null)
            return;

        bool up = wallManager.Instance.HasWall(
            gridPos + Vector2Int.up
        );

        bool down = wallManager.Instance.HasWall(
            gridPos + Vector2Int.down
        );

        bool left = wallManager.Instance.HasWall(
            gridPos + Vector2Int.left
        );

        bool right = wallManager.Instance.HasWall(
            gridPos + Vector2Int.right
        );

        TileBase correctTile = GetCorrectTile(
            up,
            down,
            left,
            right
        );

        if (correctTile == null)
            return;

        // Remove the previous tile
        wallTilemap.ClearAllTiles();

        // Put the correct tile in the center
        wallTilemap.SetTile(
            Vector3Int.zero,
            correctTile
        );
    }

    private TileBase GetCorrectTile(
        bool up,
        bool down,
        bool left,
        bool right)
    {
        if (wallTiles == null || wallTiles.Length < 9)
        {
            Debug.LogError(
                "WallBehav: You need exactly 9 wall tiles assigned!"
            );

            return null;
        }

        // =====================================================
        // NO CONNECTIONS
        // =====================================================

        // 5
        //
        //     X
        //
        if (!up && !down && !left && !right)
        {
            return wallTiles[4];
        }


        // =====================================================
        // ONE CONNECTION
        // =====================================================

        // Wall is ABOVE this wall.
        //
        //     2
        //     8
        //
        // This wall is the bottom piece.
        if (up && !down && !left && !right)
        {
            return wallTiles[7]; // 8
        }

        // Wall is BELOW this wall.
        //
        //     2
        //     8
        //
        // This wall is the top piece.
        if (down && !up && !left && !right)
        {
            return wallTiles[1]; // 2
        }

        // Wall is LEFT of this wall.
        //
        //     4  6
        //
        // This wall is the right piece.
        if (left && !up && !down && !right)
        {
            return wallTiles[5]; // 6
        }

        // Wall is RIGHT of this wall.
        //
        //     4  6
        //
        // This wall is the left piece.
        if (right && !up && !down && !left)
        {
            return wallTiles[3]; // 4
        }


        // =====================================================
        // TWO CONNECTIONS - STRAIGHT
        // =====================================================

        // Vertical
        //
        //     2
        //     5
        //     8
        //
        if (up && down && !left && !right)
        {
            return wallTiles[4]; // 5
        }

        // Horizontal
        //
        //     4  5  6
        //
        if (left && right && !up && !down)
        {
            return wallTiles[4]; // 5
        }


        // =====================================================
        // TWO CONNECTIONS - CORNERS
        // =====================================================

        // UP + LEFT
        //
        //     9
        //     8?
        //
        if (up && left && !down && !right)
        {
            return wallTiles[8]; // 9
        }

        // UP + RIGHT
        if (up && right && !down && !left)
        {
            return wallTiles[6]; // 7
        }

        // DOWN + LEFT
        if (down && left && !up && !right)
        {
            return wallTiles[2]; // 3
        }

        // DOWN + RIGHT
        if (down && right && !up && !left)
        {
            return wallTiles[0]; // 1
        }


        // =====================================================
        // THREE CONNECTIONS
        // =====================================================

        // Your current 9-piece set does not have dedicated
        // T-junction tiles.
        //
        // Use center piece for now.
        if (up && down && left)
        {
            return wallTiles[4];
        }

        if (up && down && right)
        {
            return wallTiles[4];
        }

        if (up && left && right)
        {
            return wallTiles[4];
        }

        if (down && left && right)
        {
            return wallTiles[4];
        }


        // =====================================================
        // FOUR CONNECTIONS
        // =====================================================

        if (up && down && left && right)
        {
            return wallTiles[4];
        }


        // Fallback
        return wallTiles[4];
    }

    private void OnDestroy()
    {
        if (wallManager.Instance != null)
        {
            wallManager.Instance.UnregisterWall(this);
        }
    }
}