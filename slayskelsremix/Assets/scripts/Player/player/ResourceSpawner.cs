using System.Collections;
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

    [Header("Settings")]
    public Tilemap tilemap;

    public Tilemap blockedTilemap;

    public List<ResourceEntry> resources =
        new List<ResourceEntry>();

    [Header("TREE ID")]
    public string treeIDPrefix = "tree_";

    void Start()
    {
        StartCoroutine(SpawnAfterLoad());
    }

    IEnumerator SpawnAfterLoad()
    {
        // wait for BuildingSaveManager load
        yield return new WaitForSeconds(0.1f);

        SpawnResources();
    }

    void SpawnResources()
    {
        if (BuildingSaveManager.Instance == null)
            return;

        var save =
            BuildingSaveManager.Instance
            .GetSaveData();

        BoundsInt bounds =
            tilemap.cellBounds;

        foreach (ResourceEntry entry in resources)
        {
            List<Vector3> validPositions =
                new List<Vector3>();

            // =========================================
            // COLLECT VALID TILES
            // =========================================

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(pos))
                    continue;

                if (tilemap.GetTile(pos) != entry.tile)
                    continue;

                if (IsBlocked(pos))
                    continue;

                validPositions.Add(
                    tilemap.GetCellCenterWorld(pos));
            }

            // =========================================
            // RANDOMIZE POSITIONS
            // FIXES TREE LINES
            // =========================================

            Shuffle(validPositions);

            int spawned = 0;

            foreach (Vector3 pos in validPositions)
            {
                if (spawned >= entry.maxSpawnCount)
                    break;

                // =====================================
                // DISTANCE CHECK
                // =====================================

                if (IsTooClose(pos, entry.minDistance))
                    continue;

                // =====================================
                // UNIQUE TREE ID
                // =====================================

                string id =
                    $"{treeIDPrefix}" +
                    $"{Mathf.RoundToInt(pos.x)}_" +
                    $"{Mathf.RoundToInt(pos.y)}";

                // =====================================
                // IMPORTANT:
                // DO NOT RESPAWN SAVED TREES
                // =====================================

                bool alreadyExists =
                    save != null &&
                    save.trees.Exists(
                        t => t.treeID == id);

                if (alreadyExists)
                    continue;

                // =====================================
                // FINAL OVERLAP SAFETY
                // =====================================

                Collider2D hit =
                    Physics2D.OverlapCircle(
                        pos,
                        0.2f);

                if (hit != null)
                    continue;

                // =====================================
                // SPAWN TREE
                // =====================================

                GameObject obj =
                    Instantiate(
                        entry.prefab,
                        pos,
                        Quaternion.identity);

                if (obj.TryGetComponent(
                    out treeItemBehav tree))
                {
                    tree.UniqOverworldItemID = id;
                }

                spawned++;
            }
        }

        // SAVE NEWLY SPAWNED TREES
        BuildingSaveManager.Instance
            .SaveAfterChange();
    }

    // =====================================================
    // SHUFFLE
    // =====================================================

    void Shuffle(List<Vector3> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand =
                Random.Range(i, list.Count);

            Vector3 temp = list[i];

            list[i] = list[rand];

            list[rand] = temp;
        }
    }

    // =====================================================
    // BLOCKED TILE CHECK
    // =====================================================

    bool IsBlocked(Vector3Int cellPos)
    {
        return blockedTilemap != null &&
               blockedTilemap.HasTile(cellPos);
    }

    // =====================================================
    // DISTANCE CHECK
    // =====================================================

    bool IsTooClose(Vector3 pos, float minDist)
    {
        float sqr =
            minDist * minDist;

        foreach (treeItemBehav tree in
            FindObjectsByType<treeItemBehav>(
                FindObjectsSortMode.None))
        {
            if ((tree.transform.position - pos)
                .sqrMagnitude < sqr)
            {
                return true;
            }
        }

        return false;
    }
}