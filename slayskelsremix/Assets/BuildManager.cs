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
    private buildSO currentItem;

    private SpriteRenderer[] renderers;
    private bool isPlacing = false;

    void Update()
    {
        if (!isPlacing)
            return;

        if (PlayerInputHandler.Instance == null)
            return;

        // 🔥 HIDE PREVIEW IF NO ITEMS LEFT
        if (!HasItemInInventory())
        {
            Cancel();
            return;
        }

        MovePreview();
        UpdateColor();

        if (PlayerInputHandler.Instance.LeftClickPressed())
        {
            TryPlace();
        }

        if (PlayerInputHandler.Instance.RightClickPressed() || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cancel();
        }

        // 🔥 UNDO
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            if (BuildingSaveManager.Instance != null)
                BuildingSaveManager.Instance.UndoLastBuilding();
        }
    }

    public void StartPlacing(buildSO item)
    {
        if (item == null || item.placeablePrefab == null)
            return;

        // 🔥 CHECK INVENTORY BEFORE STARTING PREVIEW
        if (!HasItemInInventory(item))
        {
            Debug.LogWarning("[Build] No items available, preview not shown.");
            return;
        }

        Cancel();

        currentItem = item;
        isPlacing = true;

        previewObject = Instantiate(item.placeablePrefab);

        Collider2D c = previewObject.GetComponent<Collider2D>();
        if (c != null) c.enabled = false;

        renderers = previewObject.GetComponentsInChildren<SpriteRenderer>();

        SetTint(new Color(1, 1, 1, 0.5f));
    }

    // ---------------- INVENTORY CHECK ----------------

    private bool HasItemInInventory()
    {
        return HasItemInInventory(currentItem);
    }

    private bool HasItemInInventory(buildSO item)
    {
        if (InvUI.Instance == null || InvUI.Instance.inventoryManager == null)
            return false;

        foreach (var slot in InvUI.Instance.inventoryManager.inventory)
        {
            if (slot.item == item && slot.count > 0)
                return true;
        }

        return false;
    }

    // ---------------- PREVIEW MOVEMENT ----------------

    private void MovePreview()
    {
        if (previewObject == null) return;

        Vector2 mousePos = PlayerInputHandler.Instance.GetMousePosition();
        float distance = Mathf.Abs(playerCamera.transform.position.z);

        Vector3 worldPos = playerCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));
        worldPos.z = 0f;

        float snapX = Mathf.Round(worldPos.x / gridSize) * gridSize;
        float snapY = Mathf.Round(worldPos.y / gridSize) * gridSize;

        previewObject.transform.position = new Vector3(snapX, snapY, 0f);
    }

    // ---------------- VALIDATION ----------------

    private bool CanPlace()
    {
        if (previewObject == null) return false;

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
        bool valid = CanPlace() && HasItemInInventory();
        Color c = valid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        SetTint(c);
    }

    private void SetTint(Color c)
    {
        if (renderers == null) return;

        foreach (var r in renderers)
        {
            if (r != null)
                r.color = c;
        }
    }

    // ---------------- PLACE ----------------

    private void TryPlace()
    {
        if (!CanPlace())
            return;

        if (!HasItemInInventory())
        {
            Debug.LogWarning("[Build] No item left!");
            Cancel();
            return;
        }

        // 1. Spawn the object
        GameObject obj = Instantiate(
            currentItem.placeablePrefab,
            previewObject.transform.position,
            Quaternion.identity
        );

        // 2. Setup identity for saving
        BuildIdentity identity = obj.GetComponent<BuildIdentity>();
        if (identity == null) identity = obj.AddComponent<BuildIdentity>();
        identity.item = currentItem;

        // 3. Register and Save
        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);
            BuildingSaveManager.Instance.SaveNow();
        }

        // 4. 🔥 THE FIX: Remove from data AND update the visual UI
        if (InvUI.Instance != null && InvUI.Instance.inventoryManager != null)
        {
            // This changes the number in your Inventory list
            InvUI.Instance.inventoryManager.RemoveItem(currentItem, 1);

            // This forces every InventorySlotUI to run SetSlot() again with new numbers
            InvUI.Instance.RefreshUI();
        }

        Debug.Log("[Build] Placed and UI Refreshed");
    }

    // ---------------- CANCEL ----------------

    private void Cancel()
    {
        if (previewObject != null)
            Destroy(previewObject);

        previewObject = null;
        currentItem = null;
        isPlacing = false;
    }
}