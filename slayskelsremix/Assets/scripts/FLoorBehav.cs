using UnityEngine;

public class FloorBehav : MonoBehaviour
{
    private Vector2Int gridPos;

    public GameObject wallPrefab;

    private GameObject frontWall;
    private GameObject bottomWall;

    private void Start()
    {
        gridPos = Vector2Int.RoundToInt(transform.position);

        FloorManager.Instance.Register(gridPos, this);
    }

    public void UpdateEdges()
    {
        CheckWall(Vector2Int.up, ref frontWall);     // front edge
        CheckWall(Vector2Int.down, ref bottomWall);  // bottom edge
    }

    void CheckWall(Vector2Int dir, ref GameObject wall)
    {
        Vector2Int neighbor = gridPos + dir;

        bool hasFloor = FloorManager.Instance.HasFloor(neighbor);

        if (!hasFloor)
        {
            if (wall == null)
            {
                Vector3 spawnPos = transform.position + new Vector3(dir.x, dir.y, 0) * 0.5f;

                wall = Instantiate(wallPrefab, spawnPos, Quaternion.identity, transform);
            }
        }
        else
        {
            if (wall != null)
            {
                Destroy(wall);
                wall = null;
            }
        }
    }

    private void OnDestroy()
    {
        if (FloorManager.Instance != null)
            FloorManager.Instance.Remove(gridPos);
    }
}