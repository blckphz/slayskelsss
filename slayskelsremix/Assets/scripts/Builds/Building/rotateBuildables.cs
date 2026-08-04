using UnityEngine;

public class RotateBuildables : MonoBehaviour
{
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;

    [SerializeField] private Transform colliderTransform;

    private SpriteRenderer spriteRenderer;
    private Quaternion originalColliderRotation;

    public bool UsingSecondSprite { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteA != null)
            spriteRenderer.sprite = spriteA;

        if (colliderTransform != null)
            originalColliderRotation = colliderTransform.localRotation;
    }

    public void ToggleSprite()
    {
        ApplyState(!UsingSecondSprite);
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

        if (colliderTransform != null)
        {
            colliderTransform.localRotation = second
                ? Quaternion.Euler(0, 0, 90f)
                : originalColliderRotation;
        }
    }
}