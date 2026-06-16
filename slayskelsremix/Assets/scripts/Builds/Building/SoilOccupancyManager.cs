using System.Collections.Generic;
using UnityEngine;

public class SoilOccupancyManager : MonoBehaviour
{
    public static SoilOccupancyManager Instance;

    [System.Serializable]
    public class CellDebug
    {
        public Vector3Int cell;
        public List<BuildIdentity> objects = new();
    }

    [Header("DEBUG - Visible in Inspector")]
    public List<CellDebug> debugCells = new();

    private Dictionary<Vector3Int, HashSet<BuildIdentity>> map = new();

    void Awake()
    {
        Instance = this;
    }

    void OnValidate()
    {
        RefreshDebug();
    }

    void RefreshDebug()
    {
        debugCells.Clear();

        foreach (var kvp in map)
        {
            CellDebug cd = new CellDebug();
            cd.cell = kvp.Key;
            cd.objects = new List<BuildIdentity>(kvp.Value);
            debugCells.Add(cd);
        }
    }

    public void Register(Vector3Int cell, BuildIdentity obj)
    {
        if (!map.TryGetValue(cell, out var set))
        {
            set = new HashSet<BuildIdentity>();
            map[cell] = set;
        }

        set.Add(obj);
        RefreshDebug();
    }

    public void Unregister(Vector3Int cell, BuildIdentity obj)
    {
        if (!map.TryGetValue(cell, out var set))
            return;

        set.Remove(obj);

        if (set.Count == 0)
            map.Remove(cell);

        RefreshDebug();
    }

    public List<BuildIdentity> Get(Vector3Int cell)
    {
        if (map.TryGetValue(cell, out var set))
            return new List<BuildIdentity>(set);

        return null;
    }

    public void RemoveAllOnCell(Vector3Int cell)
    {
        if (!map.TryGetValue(cell, out var set))
            return;

        foreach (var obj in set)
        {
            if (obj != null)
                Destroy(obj.gameObject);
        }

        map.Remove(cell);
        RefreshDebug();
    }
}