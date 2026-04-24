using System.Collections.Generic;
using UnityEngine;

public class FenceManager : MonoBehaviour
{
    public static FenceManager Instance;

    // GRID STORAGE
    private Dictionary<Vector2Int, FenceBehav> fences = new Dictionary<Vector2Int, FenceBehav>();

    private void Awake()
    {
        Instance = this;
    }

    // =========================
    // REGISTER FENCE
    // =========================
    public void Register(Vector2Int pos, FenceBehav fence)
    {
        fences[pos] = fence;
        UpdateNeighbors(pos);
    }

    // =========================
    // REMOVE FENCE
    // =========================
    public void Remove(Vector2Int pos)
    {
        if (fences.ContainsKey(pos))
            fences.Remove(pos);

        UpdateNeighbors(pos);
    }

    // =========================
    // GET NEIGHBOR CHECK
    // =========================
    public bool HasFence(Vector2Int pos)
    {
        return fences.ContainsKey(pos);
    }

    // =========================
    // UPDATE SURROUNDINGS
    // =========================
    public void UpdateNeighbors(Vector2Int pos)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };

        // update self + neighbors
        foreach (Vector2Int dir in dirs)
        {
            Vector2Int check = pos + dir;

            if (fences.TryGetValue(check, out FenceBehav fence))
            {
                fence.UpdateShape();
            }
        }

        // also update self if it exists
        if (fences.TryGetValue(pos, out FenceBehav self))
        {
            self.UpdateShape();
        }
    }
}