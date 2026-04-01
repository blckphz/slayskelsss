using UnityEngine;

public class FenceBehav : MonoBehaviour
{
    [Header("Sprites (0-15)")]
    [Tooltip("Order: 0=None, 1=R, 2=L, 3=RL, 4=U, 5=UR, 6=UL, 7=URL, 8=D, 9=DR, 10=DL, 11=DRL, 12=UD, 13=UDR, 14=UDL, 15=UDRL")]
    public Sprite[] fenceSprites;

    public LayerMask fenceLayer;
    private SpriteRenderer spriteRenderer;
    private float checkRadius = 0.2f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateShape();
        NotifyNeighbors();
    }

    public void UpdateShape()
    {
        int mask = 0;

        if (CheckDir(Vector2.right)) mask += 1;
        if (CheckDir(Vector2.left)) mask += 2;
        if (CheckDir(Vector2.up)) mask += 4;
        if (CheckDir(Vector2.down)) mask += 8;

        if (mask < fenceSprites.Length && fenceSprites[mask] != null)
        {
            spriteRenderer.sprite = fenceSprites[mask];
        }
    }

    bool CheckDir(Vector2 dir)
    {
        // Check 1 unit away (your grid size)
        Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position + dir, checkRadius, fenceLayer);
        return hit != null && hit.gameObject != this.gameObject;
    }

    void NotifyNeighbors()
    {
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (Vector2 d in dirs)
        {
            Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position + d, checkRadius, fenceLayer);
            if (hit != null)
            {
                var neighbor = hit.GetComponent<FenceBehav>();
                if (neighbor != null) neighbor.UpdateShape();
            }
        }
    }

    private void OnDestroy()
    {
        // When deleted, tell neighbors to "disconnect" from this spot
        NotifyNeighbors();
    }
}