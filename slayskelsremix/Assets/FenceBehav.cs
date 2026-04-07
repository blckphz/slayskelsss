using UnityEngine;

public class FenceBehav : MonoBehaviour
{
    public Sprite[] fenceSprites;

    private SpriteRenderer sr;
    private BoxCollider2D col;

    public Vector2 defaultSize = new Vector2(1f, 1f);
    public Vector2 elongatedSize = new Vector2(1f, 1.5f);

    private Vector2Int gridPos;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        gridPos = Vector2Int.RoundToInt(transform.position);

        FenceManager.Instance.Register(gridPos, this);
    }

    public void UpdateShape()
    {
        int mask = 0;

        if (FenceManager.Instance.HasFence(gridPos + Vector2Int.right)) mask |= 1;
        if (FenceManager.Instance.HasFence(gridPos + Vector2Int.left)) mask |= 2;
        if (FenceManager.Instance.HasFence(gridPos + Vector2Int.up)) mask |= 4;
        if (FenceManager.Instance.HasFence(gridPos + Vector2Int.down)) mask |= 8;

        if (fenceSprites != null && mask < fenceSprites.Length && fenceSprites[mask] != null)
        {
            sr.sprite = fenceSprites[mask];

            col.size = (mask == 12 || mask == 3) ? elongatedSize : defaultSize;
        }
    }

    private void OnDestroy()
    {
        if (FenceManager.Instance != null)
        {
            FenceManager.Instance.Remove(gridPos);
        }
    }
}