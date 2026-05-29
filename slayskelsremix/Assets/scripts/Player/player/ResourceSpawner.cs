using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    public Tilemap tilemap;

    // The tile you want to detect
    public TileBase targetTile;

    // Prefab to spawn
    public GameObject resourcePrefab;

    void Start()
    {
        SpawnResources();
    }

    void SpawnResources()
    {
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);

            // Check if this is the tile we're looking for
            if (tile == targetTile)
            {
                // Convert tile position to world position
                Vector3 worldPos = tilemap.GetCellCenterWorld(pos);

                // Spawn object
                Instantiate(resourcePrefab, worldPos, Quaternion.identity);
            }
        }
    }
}