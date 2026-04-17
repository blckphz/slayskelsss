using UnityEngine;

public class ghostBuildPreview : MonoBehaviour
{
    private SpriteRenderer[] renderers;

    public void InitializeGhost()
    {
        Debug.Log($"<color=cyan>[Ghost]</color> Initializing {gameObject.name}...");

        // 1. Disable all scripts (CampfireBehav, FenceBehav, objectHealth, etc.)
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != this) s.enabled = false;
        }

        // 2. Kill colliders/triggers so it doesn't block the mouse or hit the player
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders) c.enabled = false;

        // 3. Setup Renderers
        renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0)
            Debug.LogError($"<color=red>[Ghost]</color> No SpriteRenderers found on {gameObject.name}!");

        SetColor(Color.white); // Initial tint
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