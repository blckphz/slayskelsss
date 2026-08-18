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

        ARRAY

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

    private bool gridPositionInitialized = false;
    private bool registeredWithManager = false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (wallTilemap == null)
        {
            wallTilemap =
                GetComponentInChildren<Tilemap>(true);
        }

        if (wallTilemap == null)
        {
            Debug.LogError(
                "WallBehav: No Tilemap found on " +
                gameObject.name +
                ". Assign the Wall Tilemap in the Inspector."
            );

            return;
        }

        if (wallTiles == null)
        {
            Debug.LogError(
                "WallBehav: wallTiles array is NULL on " +
                gameObject.name
            );
        }
        else if (wallTiles.Length < 9)
        {
            Debug.LogError(
                "WallBehav: You need at least 9 wall tiles on " +
                gameObject.name +
                ". Current amount: " +
                wallTiles.Length
            );
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        /*
            IMPORTANT:

            For walls placed by BuildPlacer, the correct grid
            position is supplied by BuildPlacer before Start()
            executes.

            Therefore we DO NOT calculate the position from
            wallTilemap.WorldToCell(transform.position).

            That was the source of the (0,0) problem.
        */

        if (!gridPositionInitialized)
        {
            /*
                This is mainly for walls that already exist in
                the scene rather than being spawned by BuildPlacer.

                If you have a normal global Grid available, you
                can use it as a fallback.
            */

            Grid grid =
                GetComponentInParent<Grid>();

            if (grid != null)
            {
                Vector3Int cell =
                    grid.WorldToCell(
                        transform.position
                    );

                SetGridPositionInternal(
                    new Vector2Int(
                        cell.x,
                        cell.y
                    ),
                    true
                );
            }
            else
            {
                Debug.LogWarning(
                    "WallBehav: No grid position was assigned to " +
                    gameObject.name +
                    ". The wall will not register with wallManager."
                );
            }
        }
        else
        {
            UpdateShape();
        }
    }


    // =========================================================
    // SET GRID POSITION
    //
    // CALLED BY BUILDER AFTER INSTANTIATING THE PREFAB
    // =========================================================

    public void SetGridPosition(
        Vector3Int cell)
    {
        SetGridPositionInternal(
            new Vector2Int(
                cell.x,
                cell.y
            ),
            true
        );
    }


    // =========================================================
    // SET GRID POSITION INTERNAL
    // =========================================================

    private void SetGridPositionInternal(
        Vector2Int position,
        bool register)
    {
        gridPos =
            position;

        gridPositionInitialized =
            true;

        if (
            register &&
            wallManager.Instance != null)
        {
            wallManager.Instance.RegisterWall(
                this
            );

            registeredWithManager =
                true;
        }

        UpdateShape();
    }


    // =========================================================
    // GET GRID POSITION
    // =========================================================

    public Vector2Int GetGridPosition()
    {
        return gridPos;
    }


    // =========================================================
    // IS INITIALIZED
    // =========================================================

    public bool HasGridPosition()
    {
        return gridPositionInitialized;
    }


    // =========================================================
    // UPDATE SHAPE
    // =========================================================

    public void UpdateShape()
    {
        if (!gridPositionInitialized)
        {
            return;
        }

        if (wallManager.Instance == null)
        {
            return;
        }

        if (wallTilemap == null)
        {
            Debug.LogError(
                "WallBehav: wallTilemap is NULL on " +
                gameObject.name
            );

            return;
        }

        if (
            wallTiles == null ||
            wallTiles.Length < 9)
        {
            Debug.LogError(
                "WallBehav: You need at least 9 wall tiles assigned on " +
                gameObject.name
            );

            return;
        }


        // =====================================================
        // CHECK NEIGHBOURS
        // =====================================================

        bool up =
            wallManager.Instance.HasWall(
                gridPos + Vector2Int.up
            );

        bool down =
            wallManager.Instance.HasWall(
                gridPos + Vector2Int.down
            );

        bool left =
            wallManager.Instance.HasWall(
                gridPos + Vector2Int.left
            );

        bool right =
            wallManager.Instance.HasWall(
                gridPos + Vector2Int.right
            );


        // =====================================================
        // GET TILE
        // =====================================================

        TileBase correctTile =
            GetCorrectTile(
                up,
                down,
                left,
                right
            );

        if (correctTile == null)
        {
            Debug.LogError(
                "WallBehav: GetCorrectTile returned NULL for " +
                gameObject.name
            );

            return;
        }


        // =====================================================
        // CLEAR
        // =====================================================

        wallTilemap.ClearAllTiles();


        // =====================================================
        // PLACE TILE
        // =====================================================

        wallTilemap.SetTile(
            Vector3Int.zero,
            correctTile
        );


        // =====================================================
        // REFRESH
        // =====================================================

        wallTilemap.RefreshAllTiles();


        TilemapRenderer renderer =
            wallTilemap.GetComponent<TilemapRenderer>();

        if (renderer != null)
        {
            renderer.enabled = true;
        }
    }


    // =========================================================
    // GET CORRECT TILE
    // =========================================================

    private TileBase GetCorrectTile(
        bool up,
        bool down,
        bool left,
        bool right)
    {
        if (
            wallTiles == null ||
            wallTiles.Length < 9)
        {
            return null;
        }


        // =====================================================
        // NO CONNECTIONS
        // =====================================================

        if (
            !up &&
            !down &&
            !left &&
            !right)
        {
            return wallTiles[4];
        }


        // =====================================================
        // ONE CONNECTION
        // =====================================================

        // UP
        if (
            up &&
            !down &&
            !left &&
            !right)
        {
            return wallTiles[7];
        }

        // DOWN
        if (
            down &&
            !up &&
            !left &&
            !right)
        {
            return wallTiles[1];
        }

        // LEFT
        if (
            left &&
            !up &&
            !down &&
            !right)
        {
            return wallTiles[5];
        }

        // RIGHT
        if (
            right &&
            !up &&
            !down &&
            !left)
        {
            return wallTiles[3];
        }


        // =====================================================
        // TWO CONNECTIONS - STRAIGHT
        // =====================================================

        // VERTICAL
        if (
            up &&
            down &&
            !left &&
            !right)
        {
            return wallTiles[4];
        }

        // HORIZONTAL
        if (
            left &&
            right &&
            !up &&
            !down)
        {
            return wallTiles[4];
        }


        // =====================================================
        // CORNERS
        // =====================================================

        // UP + LEFT
        if (
            up &&
            left &&
            !down &&
            !right)
        {
            return wallTiles[8];
        }

        // UP + RIGHT
        if (
            up &&
            right &&
            !down &&
            !left)
        {
            return wallTiles[6];
        }

        // DOWN + LEFT
        if (
            down &&
            left &&
            !up &&
            !right)
        {
            return wallTiles[2];
        }

        // DOWN + RIGHT
        if (
            down &&
            right &&
            !up &&
            !left)
        {
            return wallTiles[0];
        }


        // =====================================================
        // THREE CONNECTIONS
        // =====================================================

        if (
            up &&
            down &&
            left &&
            !right)
        {
            return wallTiles[4];
        }

        if (
            up &&
            down &&
            right &&
            !left)
        {
            return wallTiles[4];
        }

        if (
            up &&
            left &&
            right &&
            !down)
        {
            return wallTiles[4];
        }

        if (
            down &&
            left &&
            right &&
            !up)
        {
            return wallTiles[4];
        }


        // =====================================================
        // FOUR CONNECTIONS
        // =====================================================

        if (
            up &&
            down &&
            left &&
            right)
        {
            return wallTiles[4];
        }


        // =====================================================
        // FALLBACK
        // =====================================================

        return wallTiles[4];
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (
            registeredWithManager &&
            wallManager.Instance != null)
        {
            wallManager.Instance.UnregisterWall(
                this
            );
        }

        registeredWithManager =
            false;
    }
}