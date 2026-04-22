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
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        gameObject.tag = "Untagged";

        foreach (Transform child in transform)
            child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in scripts)
            if (s != null && s != this)
                s.enabled = false;

        var buildId = GetComponent<BuildIdentity>();
        if (buildId != null)
            Destroy(buildId);

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders)
            c.enabled = false;

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
                sr.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }

    // ---------------- GRID HIT CHECK ----------------

    public List<Collider2D> GetObstacles()
    {
        detectedObstacles.Clear();

        Vector3 origin = transform.position;

        // 🔥 IMPORTANT: convert bottom-center pivot → bottom-left anchor
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
                    detectedObstacles.Add(hit);
            }
        }

        return detectedObstacles;
    }
}