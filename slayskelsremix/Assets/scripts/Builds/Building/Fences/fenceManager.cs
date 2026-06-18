using System.Collections.Generic;
using UnityEngine;

public class FenceManager : MonoBehaviour
{
    public static FenceManager Instance;

    private Dictionary<Vector2Int, FenceBehav> fences = new();
    private HashSet<Vector2Int> gates = new();

    private void Awake()
    {
        Instance = this;
    }

    // =========================
    // FENCES
    // =========================

    public void Register(Vector2Int pos, FenceBehav fence)
    {
        fences[pos] = fence;
        UpdateNeighbors(pos);
    }

    public void Remove(Vector2Int pos)
    {
        fences.Remove(pos);
        UpdateNeighbors(pos);
    }

    public bool HasFence(Vector2Int pos)
    {
        return fences.ContainsKey(pos);
    }

    // =========================
    // GATES
    // =========================

    /*

    public void RegisterGate(Vector2Int pos)
    {
        gates.Add(pos);
        UpdateNeighbors(pos);
    }

    */

    public void RemoveGate(Vector2Int pos)
    {
        gates.Remove(pos);
        UpdateNeighbors(pos);
    }

    public bool HasGate(Vector2Int pos)
    {
        return gates.Contains(pos);
    }

    // Fence OR Gate
    public bool HasConnection(Vector2Int pos)
    {
        return fences.ContainsKey(pos) || gates.Contains(pos);
    }

    // =========================
    // UPDATE
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

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int check = pos + dir;

            if (fences.TryGetValue(check, out FenceBehav fence))
            {
                fence.UpdateShape();
            }
        }

        if (fences.TryGetValue(pos, out FenceBehav self))
        {
            self.UpdateShape();
        }
    }
}