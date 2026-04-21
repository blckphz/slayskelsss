using UnityEngine;

public class TentBehav : MonoBehaviour
{
    [Header("Settings")]
    [Range(0f, 1f)]
    public float fadedOpacity = 0.5f; // How transparent it gets
    public float fullOpacity = 1.0f;  // Normal state

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Cache the SpriteRenderer component at the start
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering is the Player
        if (other.CompareTag("Player"))
        {
            SetOpacity(fadedOpacity);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the object leaving is the Player
        if (other.CompareTag("Player"))
        {
            SetOpacity(fullOpacity);
        }
    }

    void SetOpacity(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}