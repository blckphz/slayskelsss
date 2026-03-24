using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
        // 1. Monitor the hotbar selection
        CheckHotbarSelection();

        if (!isPlacing || currentItem == null)
            return;

        if (PlayerInputHandler.Instance == null)
            return;

        // 2. Continuous Logic
        MovePreview();

        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        UpdatePreviewVisuals(isOverUI);

        if (isOverUI) return;

        // 3. Input Handling
        if (PlayerInputHandler.Instance.LeftClickPressed())
        {
            TryPlace();
        }

        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            if (BuildingSaveManager.Instance != null)
                BuildingSaveManager.Instance.UndoLastBuilding();
        }
    }

    private void CheckHotbarSelection()
    {
        if (PlayerHotbarManager.Instance == null) return;

        // Get what is currently highlighted in the hotbar
        ItemData selectedItem = PlayerHotbarManager.Instance.GetSelectedItem();

        if (selectedItem is buildSO buildItem)
        {
            // If we switched to a NEW buildable item, or just started selecting one
            if (currentItem != buildItem)
            {
                StartPlacing(buildItem);
            }
        }
        else
        {
            // If the selected item is NOT buildable, clear the preview
            if (isPlacing) Cancel();
        }
    }

    public void StartPlacing(buildSO item)
    {
        Cancel(); // Clean up any existing preview

        if (item == null || item.placeablePrefab == null) return;

        currentItem = item;
        isPlacing = true;

        previewObject = Instantiate(item.placeablePrefab);

        // Disable preview collision so it doesn't block the placement raycast
        Collider2D c = previewObject.GetComponent<Collider2D>();
        if (c != null) c.enabled = false;

        renderers = previewObject.GetComponentsInChildren<SpriteRenderer>();
        SetTint(new Color(1, 1, 1, 0.5f));
    }

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

    private void UpdatePreviewVisuals(bool blockedByUI)
    {
        if (renderers == null) return;

        bool shouldBeVisible = !blockedByUI;

        foreach (var r in renderers)
        {
            if (r != null) r.enabled = shouldBeVisible;
        }

        if (shouldBeVisible)
        {
            bool valid = CanPlace();
            Color c = valid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            SetTint(c);
        }
    }

    private void SetTint(Color c)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
        {
            if (r != null) r.color = c;
        }
    }

    public void TryPlace()
    {
        if (!CanPlace()) return;

        // Double check we still have items in the hotbar
        if (PlayerHotbarManager.Instance.GetSelectedCount() <= 0) return;

        GameObject obj = Instantiate(currentItem.placeablePrefab, previewObject.transform.position, Quaternion.identity);

        BuildIdentity identity = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
        identity.item = currentItem;

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);
            BuildingSaveManager.Instance.SaveNow();
        }

        // 🔥 Remove 1 from the active hotbar slot
        PlayerHotbarManager.Instance.UseSelectedStack(1);
    }

    private void Cancel()
    {
        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        currentItem = null;
        isPlacing = false;
    }
}