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
    private bool isPlacing = false;

    void Update()
    {
        CheckHotbarSelection();

        if (!isPlacing || currentItem == null || previewObject == null) return;

        MovePreview();

        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        UpdatePreviewVisuals(isOverUI);

        if (!isOverUI && PlayerInputHandler.Instance != null && PlayerInputHandler.Instance.LeftClickPressed())
        {
            TryPlace();
        }
    }

    private void CheckHotbarSelection()
    {
        if (PlayerHotbarManager.Instance == null) return;
        ItemData selectedItem = PlayerHotbarManager.Instance.GetSelectedItem();

        if (selectedItem is buildSO buildItem)
        {
            // FIX: Compare by itemName to prevent the infinite 'Switching Ghost' loop
            if (currentItem == null || currentItem.itemName != buildItem.itemName)
            {
                Debug.Log($"<color=yellow>[BuildManager]</color> Detected Selection Change: {buildItem.itemName}");
                StartPlacing(buildItem);
            }
        }
        else if (isPlacing)
        {
            Cancel();
        }
    }

    public void StartPlacing(buildSO item)
    {
        Cancel();
        if (item == null || item.placeablePrefab == null) return;

        currentItem = item;
        isPlacing = true;

        previewObject = Instantiate(item.placeablePrefab);
        
        // Ensure the ghost component is present
        ghostBuildPreview ghost = previewObject.GetComponent<ghostBuildPreview>();
        if (ghost != null)
        {
            ghost.InitializeGhost();
        }
        else
        {
            Debug.LogWarning($"<color=orange>[BuildManager]</color> {item.itemName} Prefab is missing ghostBuildPreview script!");
        }

        Debug.Log($"<color=cyan>[BuildManager]</color> Ghost Instance Created: {previewObject.name}");
    }

    private void MovePreview()
    {
        Vector2 mousePos = PlayerInputHandler.Instance.GetMousePosition();
        
        // Ensure we are using the correct Z distance for ScreenToWorldPoint
        float camDist = Mathf.Abs(playerCamera.transform.position.z);
        Vector3 worldPos = playerCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camDist));
        
        float snapX = Mathf.Round(worldPos.x / gridSize) * gridSize;
        float snapY = Mathf.Round(worldPos.y / gridSize) * gridSize;

        // Force Z to 0 so it stays on the 2D plane
        previewObject.transform.position = new Vector3(snapX, snapY, 0f);
        
        // Debug Line: Shows in Scene view where the script THINKs the mouse is
        Debug.DrawLine(playerCamera.transform.position, previewObject.transform.position, Color.yellow);
    }

    private void UpdatePreviewVisuals(bool blockedByUI)
    {
        ghostBuildPreview ghost = previewObject.GetComponent<ghostBuildPreview>();
        if (ghost == null) return;

        if (blockedByUI)
        {
            ghost.SetColor(new Color(0,0,0,0)); // Fully transparent
            return;
        }

        bool canPlace = CanPlace();
        ghost.SetColor(canPlace ? Color.green : Color.red);
    }

    private bool CanPlace()
    {
        Collider2D col = previewObject.GetComponent<Collider2D>();
        if (col == null) return true;

        // Box check slightly smaller than the object to allow tight fitting
        Collider2D hit = Physics2D.OverlapBox(col.bounds.center, col.bounds.size * 0.9f, 0f, placementMask);
        
        if (hit != null)
        {
           // Debug.Log($"<color=red>[Placement Blocked]</color> Overlapping: {hit.name}");
            return false;
        }
        return true;
    }

    public void TryPlace()
    {
        if (previewObject == null || !CanPlace()) return;

        GameObject obj = Instantiate(currentItem.placeablePrefab, previewObject.transform.position, Quaternion.identity);
        
        BuildIdentity identity = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
        identity.item = currentItem;

        BuildingSaveManager.Instance?.RegisterBuilding(obj);
        BuildingSaveManager.Instance?.SaveNow();

        PlayerHotbarManager.Instance.UseSelectedStack(1);
        Debug.Log($"<color=green>[BuildManager]</color> Placed {currentItem.itemName} at {obj.transform.position}");
    }

    private void Cancel()
    {
        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        currentItem = null;
        isPlacing = false;
        Debug.Log("<color=gray>[BuildManager]</color> Placement Cancelled.");
    }
}