using UnityEngine;

public class RotateBuildables : MonoBehaviour
{
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;

    private SpriteRenderer spriteRenderer;

    public bool UsingSecondSprite { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteA != null)
            spriteRenderer.sprite = spriteA;
    }

    public void ToggleSprite()
    {
        if (spriteRenderer == null)
            return;

        UsingSecondSprite = !UsingSecondSprite;

        spriteRenderer.sprite = UsingSecondSprite
            ? spriteB
            : spriteA;
    }

    public void ApplyState(bool second)
    {
        UsingSecondSprite = second;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = second
                ? spriteB
                : spriteA;
        }
    }
}