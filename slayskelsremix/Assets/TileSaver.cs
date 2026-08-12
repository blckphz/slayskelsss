using UnityEngine;
using UnityEngine.Tilemaps;


public class TileSaver : MonoBehaviour
{
    public static TileSaver Instance;


    // =====================================================
    // TILEMAPS
    // =====================================================

    [Header("Tilemaps To Save")]

    public Tilemap[] tilemaps;


    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        Debug.Log(
            "[TileSaver] Ready."
        );


        if (tilemaps == null)
        {
            Debug.LogError(
                "[TileSaver] Tilemaps array is NULL!"
            );

            return;
        }


        Debug.Log(
            "[TileSaver] Tilemap count = " +
            tilemaps.Length
        );


        for (
            int i = 0;
            i < tilemaps.Length;
            i++)
        {
            if (tilemaps[i] == null)
            {
                Debug.LogError(
                    "[TileSaver] Tilemap " +
                    i +
                    " is NULL!"
                );

                continue;
            }


            Debug.Log(
                "[TileSaver] Tilemap " +
                i +
                " = " +
                tilemaps[i].name
            );
        }
    }


    // =====================================================
    // GET TILEMAP
    // =====================================================

    public Tilemap GetTilemap(
        int index)
    {
        if (
            tilemaps == null ||
            index < 0 ||
            index >= tilemaps.Length)
        {
            return null;
        }


        return tilemaps[index];
    }


    // =====================================================
    // DEBUG TILEMAP CONTENT
    // =====================================================

    public void DebugTilemaps()
    {
        Debug.Log(
            "[TileSaver] ===== TILEMAP DEBUG ====="
        );


        if (tilemaps == null)
        {
            Debug.LogError(
                "[TileSaver] tilemaps == NULL"
            );

            return;
        }


        for (
            int mapIndex = 0;
            mapIndex < tilemaps.Length;
            mapIndex++)
        {
            Tilemap tilemap =
                tilemaps[mapIndex];


            if (tilemap == null)
            {
                Debug.LogError(
                    "[TileSaver] Tilemap " +
                    mapIndex +
                    " is NULL"
                );

                continue;
            }


            BoundsInt bounds =
                tilemap.cellBounds;


            int count = 0;


            foreach (
                Vector3Int cell
                in bounds.allPositionsWithin)
            {
                TileBase tile =
                    tilemap.GetTile(cell);


                if (tile == null)
                    continue;


                count++;


                Debug.Log(
                    "[TileSaver] MAP " +
                    mapIndex +
                    " | " +
                    tilemap.name +
                    " | Cell=" +
                    cell +
                    " | Tile=" +
                    tile.name
                );
            }


            Debug.Log(
                "[TileSaver] MAP " +
                mapIndex +
                " TOTAL TILES = " +
                count
            );
        }


        Debug.Log(
            "[TileSaver] ========================="
        );
    }
}