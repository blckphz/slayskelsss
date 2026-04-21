using UnityEngine;

public class ghostBuildPreview : MonoBehaviour
{
    private SpriteRenderer[] renderers;

    public void InitializeGhost()
    {
        Debug.Log($"<color=cyan>[Ghost]</color> Initializing {gameObject.name}...");

        // 1. Disable all scripts
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in scripts)
        {
            // Check if s is null AND check if it's NOT this script
            if (s != null && s != this)
            {
                s.enabled = false;
            }
        }

        // 2. Kill colliders/triggers
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders)
        {
            if (c != null) c.enabled = false;
        }

        // 3. Setup Renderers
        renderers = GetComponentsInChildren<SpriteRenderer>();

        // Safety check for empty renderers array
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogError($"<color=red>[Ghost]</color> No SpriteRenderers found on {gameObject.name}!");
        }
        else
        {
            SetColor(Color.white);
        }
    }

    public void SetColor(Color color)
    {
        if (renderers == null) return;
        foreach (var sr in renderers)
        {
            if (sr != null)
                sr.color = new Color(color.r, color.g, color.b, 0.5f); // 50% transparency
        }
    }
}