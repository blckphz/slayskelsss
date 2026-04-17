using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public LayerMask interactLayer;
    public Color highlightColor = Color.yellow;

    private Camera mainCam;

    private objectHealth currentHover;
    private SpriteRenderer currentRenderer;
    private Color originalColor;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        HandleHover();
        HandleRightClick();
    }

    private Vector2 GetMouseWorldPos()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 world = mainCam.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, Mathf.Abs(mainCam.transform.position.z))
        );

        return new Vector2(world.x, world.y);
    }

    private void HandleHover()
    {
        Vector2 point = GetMouseWorldPos();

        Collider2D hit = Physics2D.OverlapPoint(point, interactLayer);

        if (hit != null)
        {
            objectHealth newHover = hit.GetComponent<objectHealth>();

            if (newHover != currentHover)
            {
                ClearHighlight();

                if (newHover != null)
                {
                    currentHover = newHover;
                    currentRenderer = hit.GetComponent<SpriteRenderer>();

                    if (currentRenderer != null)
                    {
                        originalColor = currentRenderer.color;
                        currentRenderer.color = highlightColor;
                    }
                }
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    private void HandleRightClick()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame)
            return;

        Vector2 point = GetMouseWorldPos();

        Collider2D hit = Physics2D.OverlapPoint(point, interactLayer);

        if (hit != null)
        {
            objectHealth obj = hit.GetComponent<objectHealth>();

            if (obj != null && obj.IsPlayerInRange())
            {
                obj.Deconstruct();
            }
        }
    }

    private void ClearHighlight()
    {
        if (currentRenderer != null)
        {
            currentRenderer.color = originalColor;
        }

        currentHover = null;
        currentRenderer = null;
    }
}