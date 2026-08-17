using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ghostBuildPreview : MonoBehaviour
{
    [Header("Grid")]
    public Vector2Int footprint = Vector2Int.one;

    [Header("Unity Grid")]
    public Grid grid;

    [Header("Placement")]
    public LayerMask placementMask;

    private SpriteRenderer[] renderers;
    private Tilemap[] tilemaps;

    private readonly List<Collider2D> detectedObstacles =
        new List<Collider2D>();


    // =====================================================
    // INIT
    // =====================================================

    public void InitializeGhost()
    {
        gameObject.layer =
            LayerMask.NameToLayer("Ignore Raycast");

        gameObject.tag =
            "Untagged";


        // =================================================
        // CHILD LAYERS
        // =================================================

        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            child.gameObject.layer =
                LayerMask.NameToLayer("Ignore Raycast");
        }


        // =================================================
        // DISABLE OTHER SCRIPTS
        // =================================================

        MonoBehaviour[] scripts =
            GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script != this)
            {
                script.enabled = false;
            }
        }


        // =================================================
        // REMOVE BUILD IDENTITY
        // =================================================

        BuildIdentity identity =
            GetComponent<BuildIdentity>();

        if (identity != null)
        {
            Destroy(identity);
        }


        // =================================================
        // DISABLE GHOST COLLIDERS
        // =================================================

        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }


        // =================================================
        // FIND SPRITES
        // =================================================

        renderers =
            GetComponentsInChildren<SpriteRenderer>(true);


        // =================================================
        // FIND TILEMAPS
        // =================================================

        tilemaps =
            GetComponentsInChildren<Tilemap>(true);


        SetColor(Color.white);
    }


    // =====================================================
    // COLOR
    // =====================================================

    public void SetColor(Color color)
    {
        Color finalColor =
            new Color(
                color.r,
                color.g,
                color.b,
                0.5f
            );


        if (renderers != null)
        {
            foreach (SpriteRenderer sr in renderers)
            {
                if (sr != null)
                {
                    sr.color = finalColor;
                }
            }
        }


        if (tilemaps != null)
        {
            foreach (Tilemap tilemap in tilemaps)
            {
                if (tilemap != null)
                {
                    tilemap.color = finalColor;
                }
            }
        }
    }


    // =====================================================
    // GET OBSTACLES
    // =====================================================

    public List<Collider2D> GetObstacles()
    {
        detectedObstacles.Clear();


        if (grid == null)
        {
            return detectedObstacles;
        }


        Vector3Int centerCell =
            grid.WorldToCell(
                transform.position
            );

        centerCell.z = 0;


        int offsetX =
            Mathf.FloorToInt(
                footprint.x / 2f
            );

        int offsetY =
            Mathf.FloorToInt(
                footprint.y / 2f
            );


        Vector2 cellSize =
            new Vector2(
                Mathf.Abs(grid.cellSize.x),
                Mathf.Abs(grid.cellSize.y)
            );


        // =================================================
        // CHECK EVERY FOOTPRINT CELL
        // =================================================

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector3Int cell =
                    centerCell +
                    new Vector3Int(
                        x - offsetX,
                        y - offsetY,
                        0
                    );


                Vector3 cellCenter =
                    grid.GetCellCenterWorld(
                        cell
                    );


                Collider2D[] hits =
                    Physics2D.OverlapBoxAll(
                        cellCenter,
                        cellSize,
                        0f,
                        placementMask
                    );


                foreach (Collider2D hit in hits)
                {
                    if (hit == null)
                        continue;


                    if (!detectedObstacles.Contains(hit))
                    {
                        detectedObstacles.Add(hit);
                    }
                }
            }
        }


        return detectedObstacles;
    }


    // =====================================================
    // FULL FOOTPRINT LAYER CHECK
    // =====================================================

    public bool IsFullyOverLayer(
        LayerMask layer)
    {
        if (grid == null)
            return false;


        Vector3Int centerCell =
            grid.WorldToCell(
                transform.position
            );

        centerCell.z = 0;


        int offsetX =
            Mathf.FloorToInt(
                footprint.x / 2f
            );

        int offsetY =
            Mathf.FloorToInt(
                footprint.y / 2f
            );


        Vector2 cellSize =
            new Vector2(
                Mathf.Abs(grid.cellSize.x),
                Mathf.Abs(grid.cellSize.y)
            );


        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector3Int cell =
                    centerCell +
                    new Vector3Int(
                        x - offsetX,
                        y - offsetY,
                        0
                    );


                Vector3 cellCenter =
                    grid.GetCellCenterWorld(
                        cell
                    );


                Collider2D hit =
                    Physics2D.OverlapBox(
                        cellCenter,
                        cellSize,
                        0f,
                        layer
                    );


                if (hit == null)
                {
                    return false;
                }
            }
        }


        return true;
    }
}