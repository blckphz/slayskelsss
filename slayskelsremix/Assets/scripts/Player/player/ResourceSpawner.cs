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

    public List<ResourceEntry> resources = new List<ResourceEntry>();

    [Header("Time Reference")]
    public DayNightCycle dayNightCycle;

    private int lastDay = -1;

    void Start()
    {
        StartCoroutine(SpawnAfterLoad());

        if (dayNightCycle == null)
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();

        if (dayNightCycle != null)
            lastDay = dayNightCycle.DaysPassed;
    }

    void Update()
    {
        CheckNewDay();
    }

    void CheckNewDay()
    {
        if (dayNightCycle == null)
            return;

        if (dayNightCycle.DaysPassed != lastDay)
        {
            lastDay = dayNightCycle.DaysPassed;
            SpawnResources(); // spawn once per new day
        }
    }

    IEnumerator SpawnAfterLoad()
    {
        yield return new WaitForSeconds(0.1f);
        SpawnResources();
    }

    void SpawnResources()
    {
        if (BuildingSaveManager.Instance == null)
            return;

        var save = BuildingSaveManager.Instance.GetSaveData();
        BoundsInt bounds = tilemap.cellBounds;

        foreach (ResourceEntry entry in resources)
        {
            List<Vector3> validPositions = new List<Vector3>();

            // COLLECT VALID POSITIONS
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

            Shuffle(validPositions);

            int spawned = 0;

            foreach (Vector3 pos in validPositions)
            {
                if (spawned >= entry.maxSpawnCount)
                    break;

                if (IsTooClose(pos, entry.minDistance))
                    continue;

                bool alreadyExists =
                    save != null &&
                    (
                        save.trees.Exists(t => Vector3.Distance(t.position, pos) < 0.1f) ||
                        save.bushes.Exists(b => Vector3.Distance(b.position, pos) < 0.1f)
                    );

                if (alreadyExists)
                    continue;

                if (Physics2D.OverlapCircle(pos, 0.2f) != null)
                    continue;

                GameObject obj = Instantiate(entry.prefab, pos, Quaternion.identity);

                if (obj.TryGetComponent(out ItemHealth item))
                {
                    item.UniqOverworldItemID = System.Guid.NewGuid().ToString();
                }

                spawned++;
            }
        }

        BuildingSaveManager.Instance.SaveAfterChange();
    }

    // ================= HELPERS =================

    void Shuffle(List<Vector3> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    bool IsBlocked(Vector3Int cellPos)
    {
        return blockedTilemap != null && blockedTilemap.HasTile(cellPos);
    }

    bool IsTooClose(Vector3 pos, float minDist)
    {
        float sqr = minDist * minDist;

        foreach (var item in FindObjectsByType<ItemHealth>(FindObjectsSortMode.None))
        {
            if ((item.transform.position - pos).sqrMagnitude < sqr)
                return true;
        }

        return false;
    }
}