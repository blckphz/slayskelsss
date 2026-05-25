using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance;

    private Dictionary<Vector2Int, FloorBehav> floors = new();

    private void Awake()
    {
        Instance = this;
    }

    public void Register(Vector2Int pos, FloorBehav floor)
    {
        floors[pos] = floor;
        UpdateNeighbors(pos);
    }

    public void Remove(Vector2Int pos)
    {
        if (floors.ContainsKey(pos))
            floors.Remove(pos);

        UpdateNeighbors(pos);
    }

    public bool HasFloor(Vector2Int pos)
    {
        return floors.ContainsKey(pos);
    }

    public void UpdateNeighbors(Vector2Int pos)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };

        foreach (var dir in dirs)
        {
            Vector2Int check = pos + dir;

            if (floors.TryGetValue(check, out FloorBehav floor))
            {
                floor.UpdateEdges();
            }
        }

        if (floors.TryGetValue(pos, out FloorBehav self))
        {
            self.UpdateEdges();
        }
    }
}