using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ResourceEntry
    {
        public TileBase tile;
        public GameObject prefab;
        public int maxSpawnCount = 10;
        public float minDistance = 1.5f;
    }

    [Header("Tilemaps")]
    public Tilemap tilemap;              // spawn tiles
    public Tilemap blockedTilemap;       // dirt / diggable / blocked tiles

    [Header("Resources")]
    public List<ResourceEntry> resources = new List<ResourceEntry>();

    void Start()
    {
        SpawnResources();
    }

    // ================= SPAWN =================

    void SpawnResources()
    {
        BoundsInt bounds = tilemap.cellBounds;

        foreach (ResourceEntry entry in resources)
        {
            List<Vector3> validPositions = new List<Vector3>();

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(pos))
                    continue;

                if (tilemap.GetTile(pos) != entry.tile)
                    continue;

                if (IsBlocked(pos))
                    continue;

                validPositions.Add(tilemap.GetCellCenterWorld(pos));
            }

            List<Vector3> spawnedPositions = new List<Vector3>();

            int attempts = 0;
            int maxSpawn = Mathf.Min(entry.maxSpawnCount, validPositions.Count);

            while (spawnedPositions.Count < maxSpawn &&
                   validPositions.Count > 0 &&
                   attempts < 10000)
            {
                attempts++;

                int index = Random.Range(0, validPositions.Count);
                Vector3 candidate = validPositions[index];
                validPositions.RemoveAt(index);

                if (IsTooClose(candidate, spawnedPositions, entry.minDistance))
                    continue;

                Instantiate(entry.prefab, candidate, Quaternion.identity);
                spawnedPositions.Add(candidate);
            }
        }
    }

    // ================= BLOCK CHECK =================

    bool IsBlocked(Vector3Int cellPos)
    {
        if (blockedTilemap == null)
            return false;

        return blockedTilemap.HasTile(cellPos);
    }

    // ================= DISTANCE CHECK =================

    bool IsTooClose(Vector3 pos, List<Vector3> others, float minDist)
    {
        float minDistSqr = minDist * minDist;

        foreach (var p in others)
        {
            if ((p - pos).sqrMagnitude < minDistSqr)
                return true;
        }

        return false;
    }
}