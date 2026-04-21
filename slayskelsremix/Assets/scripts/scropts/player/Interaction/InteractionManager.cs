using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public LayerMask interactLayer;
    [Tooltip("The layer your Tents/Houses are on.")]
    public LayerMask largeStructureLayer;
    public Color highlightColor = Color.yellow;

    private Camera mainCam;
    private GameObject currentHoverObj;
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
        bool isShiftHeld = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        GameObject targetObj = null;

        if (isShiftHeld)
        {
            // Priority 1: If Shift is held, try to find a Large Structure first
            Collider2D largeHit = Physics2D.OverlapPoint(point, largeStructureLayer);
            if (largeHit != null)
            {
                targetObj = largeHit.gameObject;
            }
        }

        // Priority 2: If no large structure found (or Shift not held), look for small items
        if (targetObj == null)
        {
            // Check everything EXCEPT the large structures
            Collider2D smallHit = Physics2D.OverlapPoint(point, interactLayer & ~largeStructureLayer);
            if (smallHit != null)
            {
                targetObj = smallHit.gameObject;
            }
        }

        // Apply highlighting logic
        if (targetObj != null)
        {
            if (targetObj != currentHoverObj)
            {
                ClearHighlight();

                if (targetObj.GetComponent<objectHealth>() != null || targetObj.GetComponent<ChestInventory>() != null)
                {
                    currentHoverObj = targetObj;
                    currentRenderer = targetObj.GetComponent<SpriteRenderer>();

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
        bool isShiftHeld = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        Collider2D hit = null;

        if (isShiftHeld)
        {
            // Shift + Right Click = Focus on Tent
            hit = Physics2D.OverlapPoint(point, largeStructureLayer);
        }

        // If not holding shift OR we held shift but didn't click a tent, check for small items
        if (hit == null)
        {
            hit = Physics2D.OverlapPoint(point, interactLayer & ~largeStructureLayer);
        }

        if (hit != null)
        {
            objectHealth health = hit.GetComponent<objectHealth>();
            ChestInventory chest = hit.GetComponent<ChestInventory>();

            if (health != null && health.IsPlayerInRange())
            {
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