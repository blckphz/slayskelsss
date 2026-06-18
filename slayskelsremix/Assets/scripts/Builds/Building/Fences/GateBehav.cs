using UnityEngine;

public class GateBehav : MonoBehaviour, IInteractable
{
    [Header("Sprites")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    [Header("Collision")]
    [SerializeField] private Collider2D blockingCollider;

    private Vector2Int gridPos;
    private SpriteRenderer sr;

    private bool isOpen;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        gridPos = Vector2Int.RoundToInt(transform.position);

        //.Instance.RegisterGate(gridPos);

        UpdateGateVisuals();
    }

    public void Interact(InventoryManager inventory)
    {
        isOpen = !isOpen;
        UpdateGateVisuals();
    }

    private void UpdateGateVisuals()
    {
        sr.sprite = isOpen ? openSprite : closedSprite;

        if (blockingCollider != null)
            blockingCollider.enabled = !isOpen;
    }

    public string GetPrompt()
    {
        return isOpen ? "Close Gate" : "Open Gate";
    }

    public void OnFocus()
    {
    }

    public void OnLoseFocus()
    {
    }

    private void OnDestroy()
    {
        if (FenceManager.Instance != null)
            FenceManager.Instance.RemoveGate(gridPos);
    }
}