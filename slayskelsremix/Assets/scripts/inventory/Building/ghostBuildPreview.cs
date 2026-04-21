using UnityEngine;
using System.Collections.Generic;

public class ghostBuildPreview : MonoBehaviour
{
    private SpriteRenderer[] renderers;
    private List<Collider2D> detectedObstacles = new List<Collider2D>();
    public LayerMask placementMask;

    public void InitializeGhost()
    {
        Debug.Log($"<color=cyan>[Ghost]</color> Initializing {gameObject.name}...");

        // 1. Disable all scripts EXCEPT this one
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != null && s != this) s.enabled = false;
        }

        // 2. Setup Colliders as Triggers (to detect the "body" overlap)
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders)
        {
            if (c != null)
            {
                c.enabled = true; // Keep it on to "sense" overlaps
                c.isTrigger = true; // Make it a ghost
            }
        }

        // 3. Add a Rigidbody2D if missing (Required for Trigger detection)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Don't fall or move physically
        rb.useFullKinematicContacts = true;

        // 4. Setup Renderers
        renderers = GetComponentsInChildren<SpriteRenderer>();
        SetColor(Color.white);
    }

    public void SetColor(Color color)
    {
        if (renderers == null) return;
        foreach (var sr in renderers)
        {
            if (sr != null)
                sr.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }

    // Check if the ghost's body is physically overlapping a blocked layer
    public bool IsBlocked()
    {
        // Clean up any null references in the list (objects destroyed while overlapping)
        detectedObstacles.RemoveAll(item => item == null || !item.enabled || !item.gameObject.activeInHierarchy);
        return detectedObstacles.Count > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the thing we hit is in the placement mask, it's an obstacle
        if (((1 << other.gameObject.layer) & placementMask) != 0)
        {
            if (!detectedObstacles.Contains(other))
                detectedObstacles.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (detectedObstacles.Contains(other))
            detectedObstacles.Remove(other);
    }
}