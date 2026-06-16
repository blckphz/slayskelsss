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

    public List<ResourceEntry> resources = new();

    [Header("Blocking")]
    public LayerMask spawnBlockLayers;
    public float blockCheckRadius = 0.2f;

    [Header("Time Reference")]
    public DayNightCycle dayNightCycle;

    private int lastDay = -1;
    private bool hasLoadedSave = false;

    private void Start()
    {
        if (dayNightCycle == null)
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();

        if (dayNightCycle != null)
            lastDay = dayNightCycle.DaysPassed;

        hasLoadedSave =
            BuildingSaveManager.Instance != null &&
            BuildingSaveManager.Instance.GetSaveData() != null;

        StartCoroutine(SpawnAfterLoad());
    }

    private void Update()
    {
        CheckNewDay();
    }

    private void CheckNewDay()
    {
        if (dayNightCycle == null)
            return;

        if (dayNightCycle.DaysPassed != lastDay)
        {
            lastDay = dayNightCycle.DaysPassed;

            if (hasLoadedSave)
                StartCoroutine(SpawnResources(false));
            else
                StartCoroutine(SpawnResources(true));
        }
    }

    private IEnumerator SpawnAfterLoad()
    {
        LoadingScreen.Instance?.SetText("Preparing World...");

        yield return new WaitForSeconds(0.1f);

        if (hasLoadedSave)
        {
            LoadingScreen.Instance?.SetText("Loading Save...");
            yield return StartCoroutine(SpawnResources(false));
        }
        else
        {
            LoadingScreen.Instance?.SetText("Generating World...");
            yield return StartCoroutine(SpawnResources(true));
        }

        LoadingScreen.Instance?.SetText("Done!");

        yield return new WaitForSeconds(0.5f);

        LoadingScreen.Instance?.FinishLoading();
    }

    private IEnumerator SpawnResources(bool bulkMode)
    {
        if (BuildingSaveManager.Instance == null)
            yield break;

        var save = BuildingSaveManager.Instance.GetSaveData();
        BoundsInt bounds = tilemap.cellBounds;

        int currentType = 0;
        int totalTypes = resources.Count;

        foreach (ResourceEntry entry in resources)
        {
            currentType++;

            LoadingScreen.Instance?.SetText(
                $"Spawning {entry.prefab.name} ({currentType}/{totalTypes})..."
            );

            yield return null;

            List<Vector3> validPositions = new();

            int checkedTiles = 0;

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                checkedTiles++;

                if (checkedTiles % 250 == 0)
                    yield return null;

                if (!tilemap.HasTile(pos))
                    continue;

                if (tilemap.GetTile(pos) != entry.tile)
                    continue;

                if (IsBlocked(pos))
                    continue;

                validPositions.Add(tilemap.GetCellCenterWorld(pos));
            }

            Shuffle(validPositions);

            int targetSpawnCount =
                bulkMode ? entry.maxSpawnCount : 1;

            int spawned = 0;

            foreach (Vector3 pos in validPositions)
            {
                if (spawned >= targetSpawnCount)
                    break;

                if (spawned % 25 == 0)
                    yield return null;

                if (IsTooClose(pos, entry.minDistance))
                    continue;

                bool alreadyExists =
                    save != null &&
                    (
                        save.trees.Exists(t =>
                            Vector3.Distance(t.position, pos) < 0.1f)
                        ||
                        save.bushes.Exists(b =>
                            Vector3.Distance(b.position, pos) < 0.1f)
                    );

                if (alreadyExists)
                    continue;

                if (Physics2D.OverlapCircle(
                    pos,
                    blockCheckRadius,
                    spawnBlockLayers) != null)
                    continue;

                GameObject obj =
                    Instantiate(entry.prefab, pos, Quaternion.identity);

                if (obj.TryGetComponent(out ItemHealth item))
                {
                    item.UniqOverworldItemID =
                        System.Guid.NewGuid().ToString();
                }

                spawned++;
            }
        }

        LoadingScreen.Instance?.SetText("Saving World...");

        yield return null;

        BuildingSaveManager.Instance.SaveAfterChange();

        LoadingScreen.Instance?.SetText("Finalizing...");

        yield return null;
    }
    private void Shuffle(List<Vector3> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private bool IsBlocked(Vector3Int cellPos)
    {
        return blockedTilemap != null &&
               blockedTilemap.HasTile(cellPos);
    }

    private bool IsTooClose(Vector3 pos, float minDist)
    {
        float sqr = minDist * minDist;

        foreach (var item in
                 FindObjectsByType<ItemHealth>(
                     FindObjectsSortMode.None))
        {
            if ((item.transform.position - pos).sqrMagnitude < sqr)
                return true;
        }

        return false;
    }
}