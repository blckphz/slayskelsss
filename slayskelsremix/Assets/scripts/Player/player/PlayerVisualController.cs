using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private charSO currentChar;
    [SerializeField] private PlayerAim aimScript;
    [SerializeField] private SpriteRenderer spriteRenderer;

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
        if (currentChar == null || aimScript == null || spriteRenderer == null)
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
                ? currentChar.backsprite
                : currentChar.frontsprite;
        }

        // Flip logic:
        // - facing down (frontsprite) → flip if aiming right
        // - facing up (backsprite) → flip if aiming left (opposite of aiming right)
        spriteRenderer.flipX = facingUp ? !isAimingRight : isAimingRight;
    }

    private void UpdateSpriteImmediate()
    {
        if (currentChar == null || spriteRenderer == null)
            return;

        spriteRenderer.sprite = currentChar.frontsprite;
        spriteRenderer.flipX = false;
        facingUp = false;
    }

    public void SetCharacter(charSO newChar)
    {
        currentChar = newChar;
        UpdateSpriteImmediate();
    }
}