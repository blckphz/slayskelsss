using System.Collections.Generic;
using UnityEngine;

public class wallManager : MonoBehaviour
{
    public static wallManager Instance { get; private set; }

    private Dictionary<Vector2Int, WallBehav> walls =
        new Dictionary<Vector2Int, WallBehav>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterWall(WallBehav wall)
    {
        Vector2Int position =
            Vector2Int.RoundToInt(wall.transform.position);

        walls[position] = wall;

        // Update the newly placed wall
        UpdateWall(position);

        // Update walls around it
        UpdateNeighbors(position);
    }

    public void UnregisterWall(WallBehav wall)
    {
        Vector2Int position =
            Vector2Int.RoundToInt(wall.transform.position);

        if (walls.TryGetValue(position, out WallBehav existing))
        {
            if (existing == wall)
            {
                walls.Remove(position);
            }
        }

        // The neighbors may need to change back
        UpdateNeighbors(position);
    }

    public bool HasWall(Vector2Int position)
    {
        return walls.ContainsKey(position);
    }

    public void UpdateWall(Vector2Int position)
    {
        if (walls.TryGetValue(position, out WallBehav wall))
        {
            wall.UpdateShape();
        }
    }

    private void UpdateNeighbors(Vector2Int position)
    {
        UpdateWall(position + Vector2Int.up);
        UpdateWall(position + Vector2Int.down);
        UpdateWall(position + Vector2Int.left);
        UpdateWall(position + Vector2Int.right);
    }
}