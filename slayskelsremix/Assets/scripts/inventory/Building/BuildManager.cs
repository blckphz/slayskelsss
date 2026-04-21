using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Pathfinding;
using System.Collections;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public LayerMask placementMask;
    public BuildRadiusController radiusController;
    public InventoryManager inventory;

    [Header("Grid")]
    public float gridSize = 1f;

    [Header("Destruction")]
    public LayerMask destroyMask;
    public LayerMask largeStructureLayer;
    public float destroyInterval = 0.05f;

    private float destroyCooldown;
    private GameObject previewObject;
    private buildSO currentItem;
    private bool isPlacing = false;

    private void Start()
    {
        StartCoroutine(InitialGraphScan());
    }

    private IEnumerator InitialGraphScan()
    {
        yield return new WaitForEndOfFrame();
        if (AstarPath.active != null) AstarPath.active.Scan();
    }

    void Update()
    {
        CheckHotbarSelection();
        HandleDestruction();

        if (!isPlacing || currentItem == null || previewObject == null)
            return;

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
            if (currentItem == null || currentItem.itemName != buildItem.itemName)
                StartPlacing(buildItem);
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

        ghostBuildPreview ghost = previewObject.GetComponent<ghostBuildPreview>();
        if (ghost != null)
        {
            ghost.placementMask = placementMask; // Assign the mask
            ghost.InitializeGhost();
        }
    }

    private void MovePreview()
    {
        Vector2 mousePos = PlayerInputHandler.Instance.GetMousePosition();
        float camDist = Mathf.Abs(playerCamera.transform.position.z);
        Vector3 worldPos = playerCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camDist));

        float snapX = Mathf.Round(worldPos.x / gridSize) * gridSize;
        float snapY = Mathf.Round(worldPos.y / gridSize) * gridSize;
        previewObject.transform.position = new Vector3(snapX, snapY, 0f);
    }

    private void UpdatePreviewVisuals(bool blockedByUI)
    {
        ghostBuildPreview ghost = previewObject.GetComponent<ghostBuildPreview>();
        if (ghost == null) return;

        if (blockedByUI)
        {
            ghost.SetColor(new Color(0, 0, 0, 0));
            return;
        }

        bool canPlace = CanPlace();
        ghost.SetColor(canPlace ? Color.green : Color.red);
    }

    private bool CanPlace()
    {
        if (previewObject == null) return false;

        // 1. Radius Check
        if (radiusController != null && !radiusController.IsWithinRadius(previewObject.transform.position))
            return false;

        // 2. Body Overlap Check
        ghostBuildPreview ghost = previewObject.GetComponent<ghostBuildPreview>();
        if (ghost != null && ghost.IsBlocked())
        {
            return false; // The ghost's body is hitting something on the placementMask
        }

        return true;
    }

    public void TryPlace()
    {
        if (previewObject == null || !CanPlace()) return;

        GameObject obj = Instantiate(currentItem.placeablePrefab, previewObject.transform.position, Quaternion.identity);

        BuildIdentity identity = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
        identity.item = currentItem;

        if (QuestManager.Instance != null) QuestManager.Instance.OnBuildingPlaced(currentItem);

        UpdateAstarGraph(obj.GetComponent<Collider2D>().bounds);
        BuildingSaveManager.Instance?.RegisterBuilding(obj);
        BuildingSaveManager.Instance?.SaveNow();
        PlayerHotbarManager.Instance.UseSelectedStack(1);
    }

    private void HandleDestruction()
    {
        if (PlayerInputHandler.Instance == null || !PlayerInputHandler.Instance.RightClickHeld())
        {
            destroyCooldown = 0f;
            return;
        }

        destroyCooldown -= Time.deltaTime;
        if (destroyCooldown <= 0f)
        {
            Vector2 mousePos = PlayerInputHandler.Instance.GetMousePosition();
            Vector3 worldPos = playerCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Mathf.Abs(playerCamera.transform.position.z)));

            bool isShiftHeld = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
            Collider2D hit = null;

            if (isShiftHeld)
                hit = Physics2D.OverlapPoint(worldPos, largeStructureLayer);

            if (hit == null)
                hit = Physics2D.OverlapPoint(worldPos, destroyMask & ~largeStructureLayer);

            if (hit != null)
            {
                DestroyBuilding(hit.gameObject);
                destroyCooldown = destroyInterval;
            }
        }
    }

    private void DestroyBuilding(GameObject obj)
    {
        if (obj == null) return;
        ChestInventory chest = obj.GetComponent<ChestInventory>();
        if (chest != null && !chest.IsEmpty()) return;

        Bounds areaToUpdate = obj.GetComponent<Collider2D>().bounds;
        BuildIdentity id = obj.GetComponent<BuildIdentity>();
        if (id != null && id.item != null && inventory != null) inventory.AddItem(id.item, 1);

        BuildingSaveManager.Instance?.UnregisterBuilding(obj);
        BuildingSaveManager.Instance?.SaveNow();
        Destroy(obj);
        UpdateAstarGraph(areaToUpdate);
    }

    private void UpdateAstarGraph(Bounds bounds)
    {
        if (AstarPath.active != null)
        {
            GraphUpdateObject guo = new GraphUpdateObject(bounds);
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo);
        }
    }

    private void Cancel()
    {
        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        currentItem = null;
        isPlacing = false;
    }
}