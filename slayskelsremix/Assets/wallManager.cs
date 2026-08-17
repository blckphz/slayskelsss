using System.Collections.Generic;
using UnityEngine;


// =========================================================
// WALL MANAGER
// =========================================================

public class wallManager : MonoBehaviour
{
    public static wallManager Instance { get; private set; }


    // =========================================================
    // WALL LOOKUP
    // =========================================================

    private Dictionary<Vector2Int, WallBehav> walls =
        new Dictionary<Vector2Int, WallBehav>();


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // =========================================================
    // REGISTER WALL
    // =========================================================

    public void RegisterWall(
        WallBehav wall)
    {
        if (wall == null)
        {
            return;
        }


        if (!wall.HasGridPosition())
        {
            Debug.LogWarning(
                "[WALL MANAGER] Tried to register wall without grid position: " +
                wall.name
            );

            return;
        }


        Vector2Int position =
            wall.GetGridPosition();


        Debug.Log(
            $"<color=cyan>" +
            $"[WALL MANAGER] REGISTER WALL" +
            $"</color> | " +
            $"Wall: {wall.name} | " +
            $"Position: {position} | " +
            $"World Position: {wall.transform.position}"
        );


        // =====================================================
        // EXISTING WALL
        // =====================================================

        if (
            walls.TryGetValue(
                position,
                out WallBehav existingWall
            ))
        {
            if (
                existingWall != null &&
                existingWall != wall)
            {
                Debug.LogWarning(
                    $"[WALL MANAGER] " +
                    $"Another wall already exists at " +
                    $"{position}. " +
                    $"Replacing old wall."
                );
            }
        }


        // =====================================================
        // REGISTER
        // =====================================================

        walls[position] =
            wall;


        // =====================================================
        // UPDATE THIS WALL
        // =====================================================

        UpdateWall(
            position
        );


        // =====================================================
        // UPDATE NEIGHBOURS
        // =====================================================

        UpdateNeighbors(
            position
        );
    }


    // =========================================================
    // UNREGISTER WALL
    // =========================================================

    public void UnregisterWall(
        WallBehav wall)
    {
        if (wall == null)
        {
            return;
        }


        Vector2Int position =
            wall.GetGridPosition();


        Debug.Log(
            $"<color=red>" +
            $"[WALL MANAGER] UNREGISTER WALL" +
            $"</color> | " +
            $"Wall: {wall.name} | " +
            $"Position: {position}"
        );


        // =====================================================
        // ONLY REMOVE THE SAME INSTANCE
        // =====================================================

        if (
            walls.TryGetValue(
                position,
                out WallBehav existingWall
            ))
        {
            if (existingWall == wall)
            {
                walls.Remove(
                    position
                );
            }
        }


        // =====================================================
        // UPDATE NEIGHBOURS
        // =====================================================

        UpdateNeighbors(
            position
        );
    }


    // =========================================================
    // HAS WALL
    // =========================================================

    public bool HasWall(
        Vector2Int position)
    {
        if (
            walls.TryGetValue(
                position,
                out WallBehav wall
            ))
        {
            if (wall != null)
            {
                return true;
            }

            walls.Remove(
                position
            );
        }

        return false;
    }


    // =========================================================
    // GET WALL
    // =========================================================

    public WallBehav GetWall(
        Vector2Int position)
    {
        if (
            walls.TryGetValue(
                position,
                out WallBehav wall
            ))
        {
            if (wall != null)
            {
                return wall;
            }

            walls.Remove(
                position
            );
        }

        return null;
    }


    // =========================================================
    // UPDATE WALL
    // =========================================================

    public void UpdateWall(
        Vector2Int position)
    {
        WallBehav wall =
            GetWall(position);

        if (wall != null)
        {
            wall.UpdateShape();
        }
    }


    // =========================================================
    // UPDATE NEIGHBOURS
    // =========================================================

    private void UpdateNeighbors(
        Vector2Int position)
    {
        UpdateWall(
            position + Vector2Int.up
        );

        UpdateWall(
            position + Vector2Int.down
        );

        UpdateWall(
            position + Vector2Int.left
        );

        UpdateWall(
            position + Vector2Int.right
        );
    }


    // =========================================================
    // UPDATE WALLS AROUND CELL
    // =========================================================

    public void UpdateWallsAround(
        Vector2Int position)
    {
        Debug.Log(
            $"[WALL MANAGER] Updating walls around: " +
            position
        );


        UpdateWall(
            position
        );


        UpdateNeighbors(
            position
        );
    }


    // =========================================================
    // FORCE UPDATE ALL
    // =========================================================

    public void UpdateAllWalls()
    {
        Debug.Log(
            $"<color=yellow>" +
            $"[WALL MANAGER] Updating ALL walls. " +
            $"Count: {walls.Count}" +
            $"</color>"
        );


        List<Vector2Int> positions =
            new List<Vector2Int>(
                walls.Keys
            );


        foreach (
            Vector2Int position
            in positions)
        {
            UpdateWall(
                position
            );
        }
    }


    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearAllWalls()
    {
        walls.Clear();
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("Debug Walls")]
    public void DebugWalls()
    {
        Debug.Log(
            "<color=cyan>" +
            "========== WALL MANAGER ==========" +
            "</color>"
        );


        foreach (
            KeyValuePair<Vector2Int, WallBehav> pair
            in walls)
        {
            if (pair.Value == null)
            {
                Debug.Log(
                    $"Position: {pair.Key} | WALL NULL"
                );

                continue;
            }


            Debug.Log(
                $"Position: {pair.Key} | " +
                $"Wall: {pair.Value.name} | " +
                $"World: {pair.Value.transform.position}"
            );
        }


        Debug.Log(
            "<color=cyan>" +
            "==================================" +
            "</color>"
        );
    }
}