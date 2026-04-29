using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAim aimScript;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;

    private bool facingUp;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateSpriteImmediate();
    }

    private void Update()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (aimScript == null || spriteRenderer == null)
            return;

        if (aimScript.anchor == null)
            return;

        Vector3 aimDir = aimScript.anchor.position - transform.position;

        bool isAimingUp = aimDir.y > 0f;
        bool isAimingRight = aimDir.x > 0f;

        // Switch front/back sprite
        if (isAimingUp != facingUp)
        {
            facingUp = isAimingUp;

            spriteRenderer.sprite = facingUp
                ? backSprite
                : frontSprite;
        }

        // Flip logic:
        // front (down) → flip if aiming right
        // back (up) → flip if aiming left
        spriteRenderer.flipX = facingUp ? !isAimingRight : isAimingRight;
    }

    private void UpdateSpriteImmediate()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = frontSprite;
        spriteRenderer.flipX = false;
        facingUp = false;
    }
}