using UnityEngine;
using System.Collections.Generic;

public class ghostBuildPreview : MonoBehaviour
{
    [Header("Grid")]
    public Vector2Int footprint = Vector2Int.one;
    public float gridSize = 1f;

    public LayerMask placementMask;

    private SpriteRenderer[] renderers;
    private List<Collider2D> detectedObstacles = new List<Collider2D>();

    // ---------------- INIT ----------------

    public void InitializeGhost()
    {
        // Ensure the ghost doesn't interfere with raycasts or physics
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        gameObject.tag = "Untagged";

        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        // Disable all game logic scripts on the ghost (so machines don't start working, etc.)
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != null && s != this)
                s.enabled = false;
        }

        // Remove BuildIdentity if it exists on the ghost to avoid save-system confusion
        var buildId = GetComponent<BuildIdentity>();
        if (buildId != null)
            Destroy(buildId);

        // Disable physical colliders so the player doesn't bump into the "ghost"
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders)
        {
            c.enabled = false;
        }

        renderers = GetComponentsInChildren<SpriteRenderer>();
        SetColor(Color.white);
    }

    // ---------------- VISUAL ----------------

    public void SetColor(Color color)
    {
        if (renderers == null) return;

        foreach (var sr in renderers)
        {
            if (sr != null)
            {
                // We keep the alpha at 0.5f to maintain the "ghostly" transparency
                sr.color = new Color(color.r, color.g, color.b, 0.5f);
            }
        }
    }

    // ---------------- GRID HIT CHECK ----------------

    /// <summary>
    /// Checks the grid points based on the footprint and returns any obstacles hit.
    /// Used by BuildManager to determine if placement is valid.
    /// </summary>
    public List<Collider2D> GetObstacles()
    {
        detectedObstacles.Clear();

        Vector3 origin = transform.position;

        // Convert bottom-center pivot → bottom-left anchor for the grid loop
        Vector3 bottomLeft = origin - new Vector3(
            (footprint.x - 1) * gridSize * 0.5f,
            0f,
            0f
        );

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector2 checkPos = new Vector2(
                    bottomLeft.x + x * gridSize,
                    bottomLeft.y + y * gridSize
                );

                Collider2D hit = Physics2D.OverlapPoint(checkPos, placementMask);

                if (hit != null && !detectedObstacles.Contains(hit))
                {
                    detectedObstacles.Add(hit);
                }
            }
        }

        return detectedObstacles;
    }
}