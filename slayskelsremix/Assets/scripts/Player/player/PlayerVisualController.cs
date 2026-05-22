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
        spriteRenderer.sprite = frontSprite;
    }

    private void Update()
    {
        if (aimScript == null)
            return;

        Vector3 dir = aimScript.AimDirection;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        bool up = dir.y > 0f;
        bool right = dir.x > 0f;

        if (up != facingUp)
        {
            facingUp = up;
            spriteRenderer.sprite = facingUp ? backSprite : frontSprite;
        }

        spriteRenderer.flipX = facingUp ? !right : right;
    }
}