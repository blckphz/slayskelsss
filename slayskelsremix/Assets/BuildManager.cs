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
        CheckHotbarSelection();

        if (!isPlacing || currentItem == null)
            return;

        if (PlayerInputHandler.Instance == null)
            return;

        MovePreview();

        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        UpdatePreviewVisuals(isOverUI);

        if (isOverUI) return;

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

        ItemData selectedItem = PlayerHotbarManager.Instance.GetSelectedItem();

        if (selectedItem is buildSO buildItem)
        {
            if (currentItem != buildItem)
            {
                StartPlacing(buildItem);
            }
        }
        else
        {
            if (isPlacing) Cancel();
        }
    }

    public void StartPlacing(buildSO item)
    {
        Cancel();

        if (item == null || item.placeablePrefab == null) return;

        currentItem = item;
        isPlacing = true;

        previewObject = Instantiate(item.placeablePrefab);

        Collider2D c = previewObject.GetComponent<Collider2D>();
        if (c != null) c.enabled = false;

        renderers = previewObject.GetComponentsInChildren<SpriteRenderer>();
        SetTint(new Color(1, 1, 1, 0.5f));

        Debug.Log($"<color=cyan>[BUILD]</color> Started placing: {item.name}");
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

        Collider2D col = previewObject.GetComponent<Collider2D>();

        if (col == null)
        {
            Debug.LogWarning("[BUILD] No collider found on preview!");
            return true;
        }

        Bounds bounds = col.bounds;

        // 🔥 Shrink slightly to prevent false positives
        Vector2 checkSize = bounds.size * 0.85f;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            bounds.center,
            checkSize,
            0f,
            placementMask
        );

        if (hits.Length > 0)
        {
            foreach (var h in hits)
            {
                Debug.Log($"<color=red>[BLOCKED]</color> Hit: {h.name} (Layer: {LayerMask.LayerToName(h.gameObject.layer)})");
            }
            return false;
        }

        Debug.Log($"<color=green>[CLEAR]</color> Can place at {bounds.center}");
        return true;
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

        if (PlayerHotbarManager.Instance.GetSelectedCount() <= 0)
        {
            Debug.Log("<color=red>[BUILD]</color> No items left in hotbar!");
            return;
        }

        GameObject obj = Instantiate(currentItem.placeablePrefab, previewObject.transform.position, Quaternion.identity);

        BuildIdentity identity = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
        identity.item = currentItem;

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);
            BuildingSaveManager.Instance.SaveNow();
        }

        PlayerHotbarManager.Instance.UseSelectedStack(1);

        Debug.Log($"<color=yellow>[BUILD]</color> Placed: {currentItem.name} at {previewObject.transform.position}");
    }

    private void Cancel()
    {
        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        currentItem = null;
        isPlacing = false;
    }

    // 🔥 DRAW DEBUG BOX IN SCENE VIEW
    private void OnDrawGizmos()
    {
        if (previewObject == null) return;

        Collider2D col = previewObject.GetComponent<Collider2D>();
        if (col == null) return;

        Bounds bounds = col.bounds;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size * 0.85f);
    }
}