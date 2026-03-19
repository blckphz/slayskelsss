using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public LayerMask placementMask;

    [Header("Grid")]
    public float gridSize = 1f;

    private GameObject previewObject;
    private ItemData currentItem;

    private SpriteRenderer[] renderers;
    private bool isPlacing = false;

    // ---------------- UPDATE ----------------

    void Update()
    {
        if (!isPlacing)
            return;

        // 1. Check if Input Handler is actually there
        if (PlayerInputHandler.Instance == null)
        {
            Debug.LogError("[Build] PlayerInputHandler.Instance is MISSING from the scene!");
            return;
        }

        MovePreview();
        UpdateColor();

        // 2. Handle Clicks
        if (PlayerInputHandler.Instance.LeftClickPressed())
        {
            Debug.Log("[Build] Click detected at: " + PlayerInputHandler.Instance.GetMousePosition());
            TryPlace();
        }

        // 3. Handle Cancel
        if (PlayerInputHandler.Instance.RightClickPressed() || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("[Build] Cancel detected");
            Cancel();
        }
    }

    // ---------------- START PLACING ----------------

    public void StartPlacing(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("[Build] StartPlacing called with NULL item");
            return;
        }

        if (item.placeablePrefab == null)
        {
            Debug.LogError("[Build] Item has no prefab assigned: " + item.itemName);
            return;
        }

        currentItem = item;
        isPlacing = true;

        previewObject = Instantiate(item.placeablePrefab);

        if (previewObject == null)
        {
            Debug.LogError("[Build] Failed to instantiate preview object");
            return;
        }

        // Disable colliders on preview so it doesn't block its own CanPlace check
        Collider2D c = previewObject.GetComponent<Collider2D>();
        if (c != null) c.enabled = false;

        renderers = previewObject.GetComponentsInChildren<SpriteRenderer>();

        SetTint(new Color(1, 1, 1, 0.5f));

        Debug.Log("[Build] Started placing: " + item.itemName);
    }

    // ---------------- MOVE PREVIEW ----------------

    private void MovePreview()
    {
        if (previewObject == null || playerCamera == null)
            return;

        // Get mouse screen position (e.g., 1920x1080 coords)
        Vector2 mousePos = PlayerInputHandler.Instance.GetMousePosition();

        // Cinemachine/2D Fix: We need a reliable distance from the camera.
        // ScreenToWorldPoint needs the Z to be the distance from the camera plane to the world plane.
        float cameraZ = playerCamera.transform.position.z;
        float targetZ = 0f; // We want the object at Z = 0
        float distance = Mathf.Abs(cameraZ - targetZ);

        // Convert Screen space to World space
        Vector3 worldPos = playerCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));

        // Safety: Force worldPos Z to exactly 0 for 2D
        worldPos.z = 0f;

        // Apply Grid Snapping
        float snapX = Mathf.Round(worldPos.x / gridSize) * gridSize;
        float snapY = Mathf.Round(worldPos.y / gridSize) * gridSize;

        previewObject.transform.position = new Vector3(snapX, snapY, 0f);

        // DEBUG: If it's still in the corner, check if mousePos is (0,0)
        if (worldPos == Vector3.zero || mousePos == Vector2.zero)
        {
            Debug.LogWarning($"[Build] Potential Zero Error - Mouse: {mousePos} | World: {worldPos}");
        }
    }

    // ---------------- VALIDATION ----------------

    private bool CanPlace()
    {
        if (previewObject == null) return false;

        // Check if anything in 'placementMask' is inside a box at this position
        // We use 0.95f to avoid pixel-perfect edges blocking placement
        Collider2D hit = Physics2D.OverlapBox(
            previewObject.transform.position,
            new Vector2(gridSize * 0.95f, gridSize * 0.95f),
            0f,
            placementMask
        );

        return hit == null;
    }

    // ---------------- COLOR ----------------

    private void UpdateColor()
    {
        bool valid = CanPlace();
        Color c = valid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        SetTint(c);
    }

    private void SetTint(Color c)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
        {
            if (r != null) r.color = c;
        }
    }

    // ---------------- PLACE ----------------

    private void TryPlace()
    {
        if (!CanPlace())
        {
            Debug.LogWarning("[Build] Placement blocked!");
            return;
        }

        Instantiate(
            currentItem.placeablePrefab,
            previewObject.transform.position,
            Quaternion.identity
        );

        Debug.Log("[Build] Placed: " + currentItem.itemName);

        // Clean up
        Destroy(previewObject);
        previewObject = null;
        currentItem = null;
        isPlacing = false;
    }

    // ---------------- CANCEL ----------------

    private void Cancel()
    {
        Debug.Log("[Build] Cancelled");
        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        currentItem = null;
        isPlacing = false;
    }
}