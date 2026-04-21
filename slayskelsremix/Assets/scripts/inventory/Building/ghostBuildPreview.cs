using UnityEngine;
using System.Collections.Generic;

public class ghostBuildPreview : MonoBehaviour
{
    private SpriteRenderer[] renderers;

    private List<Collider2D> detectedObstacles = new List<Collider2D>();

    public LayerMask placementMask;

    // ---------------- INIT ----------------

    public void InitializeGhost()
    {
        // Set to Ignore Raycast so it doesn't interfere with mouse
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        // Remove tag so InteractionManager ignores it
        gameObject.tag = "Untagged";

        // Disable ALL scripts except this one
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != null && s != this)
                s.enabled = false;
        }

        // Remove identity so it isn't treated as real object
        var buildId = GetComponent<BuildIdentity>();
        if (buildId != null)
            Destroy(buildId);

        // Setup colliders as triggers
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders)
        {
            if (c != null)
            {
                c.enabled = true;
                c.isTrigger = true;
            }
        }

        // Ensure Rigidbody2D exists for trigger events
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

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

    // ---------------- OBSTACLE ACCESS ----------------

    public List<Collider2D> GetObstacles()
    {
        // Clean invalid entries
        detectedObstacles.RemoveAll(item =>
            item == null ||
            !item.enabled ||
            !item.gameObject.activeInHierarchy);

        return detectedObstacles;
    }

    public bool IsBlocked()
    {
        return GetObstacles().Count > 0;
    }

    // ---------------- TRIGGERS ----------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only track layers we care about
        if (((1 << other.gameObject.layer) & placementMask) == 0)
            return;

        if (!detectedObstacles.Contains(other))
        {
            detectedObstacles.Add(other);

            // 🔍 DEBUG (optional)
            // Debug.Log($"[Ghost] Enter: {other.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (detectedObstacles.Contains(other))
        {
            detectedObstacles.Remove(other);

            // 🔍 DEBUG (optional)
            // Debug.Log($"[Ghost] Exit: {other.name}");
        }
    }
}