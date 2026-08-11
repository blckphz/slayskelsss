using UnityEngine;
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

    private List<Collider2D> detectedObstacles =
        new List<Collider2D>();



    // ===============================
    // INIT
    // ===============================

    public void InitializeGhost()
    {
        gameObject.layer =
            LayerMask.NameToLayer("Ignore Raycast");


        gameObject.tag = "Untagged";



        foreach (Transform child in transform)
        {
            child.gameObject.layer =
                LayerMask.NameToLayer("Ignore Raycast");
        }



        MonoBehaviour[] scripts =
            GetComponentsInChildren<MonoBehaviour>();


        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script != this)
                script.enabled = false;
        }



        BuildIdentity identity =
            GetComponent<BuildIdentity>();


        if (identity != null)
            Destroy(identity);



        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>();


        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }



        renderers =
            GetComponentsInChildren<SpriteRenderer>();


        SetColor(Color.white);
    }



    // ===============================
    // COLOR
    // ===============================

    public void SetColor(Color color)
    {
        if (renderers == null)
            return;



        foreach (SpriteRenderer sr in renderers)
        {
            if (sr != null)
            {
                sr.color =
                    new Color(
                        color.r,
                        color.g,
                        color.b,
                        0.5f
                    );
            }
        }
    }





    // ===============================
    // OBSTACLE CHECK
    // ===============================

    public List<Collider2D> GetObstacles()
    {
        detectedObstacles.Clear();



        if (grid == null)
            return detectedObstacles;



        Vector3Int centerCell =
            grid.WorldToCell(
                transform.position
            );



        int offsetX =
            Mathf.FloorToInt(
                footprint.x / 2f
            );


        int offsetY =
            Mathf.FloorToInt(
                footprint.y / 2f
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



                Vector3 world =
                    grid.GetCellCenterWorld(cell);



                Collider2D hit =
                    Physics2D.OverlapPoint(
                        world,
                        placementMask
                    );



                if (hit != null &&
                    !detectedObstacles.Contains(hit))
                {
                    detectedObstacles.Add(hit);
                }
            }
        }



        return detectedObstacles;
    }





    // ===============================
    // CHECK ENTIRE FOOTPRINT LAYER
    // ===============================

    public bool IsFullyOverLayer(
        LayerMask layer)
    {
        if (grid == null)
            return false;



        Vector3Int centerCell =
            grid.WorldToCell(
                transform.position
            );



        int offsetX =
            Mathf.FloorToInt(
                footprint.x / 2f
            );


        int offsetY =
            Mathf.FloorToInt(
                footprint.y / 2f
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



                Vector3 world =
                    grid.GetCellCenterWorld(cell);



                Collider2D hit =
                    Physics2D.OverlapPoint(
                        world,
                        layer
                    );



                if (hit == null)
                    return false;
            }
        }



        return true;
    }
}