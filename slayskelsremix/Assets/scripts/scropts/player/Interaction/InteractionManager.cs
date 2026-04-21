using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public LayerMask interactLayer;
    public Color highlightColor = Color.yellow;

    private Camera mainCam;

    private GameObject currentHoverObj; // Track the GameObject instead of just objectHealth
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
            if (hit.gameObject != currentHoverObj)
            {
                ClearHighlight();

                // Highlight if it has objectHealth OR ChestInventory
                if (hit.GetComponent<objectHealth>() != null || hit.GetComponent<ChestInventory>() != null)
                {
                    currentHoverObj = hit.gameObject;
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
            objectHealth health = hit.GetComponent<objectHealth>();
            ChestInventory chest = hit.GetComponent<ChestInventory>();

            // If it has health and is in range
            if (health != null && health.IsPlayerInRange())
            {
                // Extra check here for immediate feedback
                if (chest != null && !chest.IsEmpty())
                {
                    Debug.Log("InteractionManager: Chest must be empty to pick up!");
                    return;
                }

                health.Deconstruct();
            }
        }
    }

    private void ClearHighlight()
    {
        if (currentRenderer != null)
        {
            currentRenderer.color = originalColor;
        }

        currentHoverObj = null;
        currentRenderer = null;
    }
}