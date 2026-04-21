using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Pathfinding;
using System.Collections;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform player;
    public LayerMask placementMask;
    public LayerMask largeStructureLayer;
    public BuildRadiusController radiusController;
    public InventoryManager inventory;

    [Header("Grid")]
    public float gridSize = 1f;

    private GameObject previewObject;
    private buildSO currentItem;
    private bool isPlacing = false;

    private ghostBuildPreview ghost;

    private GameObject circleObj;
    private LineRenderer circleLine;

    private const int CIRCLE_SEGMENTS = 64;

    private void Start()
    {
        StartCoroutine(InitialGraphScan());
    }

    private IEnumerator InitialGraphScan()
    {
        yield return new WaitForEndOfFrame();
        if (AstarPath.active != null)
            AstarPath.active.Scan();
    }

    void Update()
    {
        CheckHotbarSelection();

        if (!isPlacing || currentItem == null || previewObject == null)
            return;

        MovePreview();

        bool isOverUI = EventSystem.current != null &&
                        EventSystem.current.IsPointerOverGameObject();

        UpdatePreviewVisuals(isOverUI);
        UpdateRadiusCircle();

        if (!isOverUI &&
            PlayerInputHandler.Instance != null &&
            PlayerInputHandler.Instance.LeftClickPressed())
        {
            TryPlace();
        }
    }

    // ---------------- HOTBAR ----------------

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

        CreateRadiusCircle();

        ghost = previewObject.GetComponent<ghostBuildPreview>();
        if (ghost != null)
        {
            ghost.placementMask = placementMask;
            ghost.InitializeGhost();
        }
    }

    private void Cancel()
    {
        if (previewObject != null) Destroy(previewObject);
        if (circleObj != null) Destroy(circleObj);

        previewObject = null;
        circleObj = null;
        circleLine = null;
        currentItem = null;
        ghost = null;
        isPlacing = false;
    }

    // ---------------- PREVIEW ----------------

    private void MovePreview()
    {
        Vector2 mousePos = PlayerInputHandler.Instance.GetMousePosition();
        float camDist = Mathf.Abs(playerCamera.transform.position.z);

        Vector3 worldPos = playerCamera.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, camDist));

        float snapX = Mathf.Round(worldPos.x / gridSize) * gridSize;
        float snapY = Mathf.Round(worldPos.y / gridSize) * gridSize;

        Vector3 snappedPos = new Vector3(snapX, snapY, 0f);

        if (radiusController != null && player != null)
        {
            float radius = radiusController.GetRadius();

            if (radius > 0f)
            {
                Vector2 toPos = snappedPos - player.position;

                if (toPos.magnitude > radius)
                    snappedPos = player.position + (Vector3)(toPos.normalized * radius);
            }
        }

        previewObject.transform.position = snappedPos;
    }

    // ---------------- PLACE ----------------

    public void TryPlace()
    {
        if (previewObject == null || !CanPlace()) return;

        GameObject obj = Instantiate(
            currentItem.placeablePrefab,
            previewObject.transform.position,
            Quaternion.identity
        );

        // Ensure correct layer is applied AFTER placement
        obj.layer = currentItem.placeablePrefab.layer;

        BuildIdentity identity = obj.GetComponent<BuildIdentity>() ??
                                 obj.AddComponent<BuildIdentity>();

        identity.item = currentItem;

        PlayerHotbarManager.Instance.UseSelectedStack(1);
    }

    private bool CanPlace()
    {
        if (previewObject == null) return false;

        Vector3 pos = previewObject.transform.position;

        // Radius check
        if (radiusController != null &&
            !radiusController.IsWithinRadius(pos))
            return false;

        if (ghost == null) return false;

        int itemLayer = currentItem.placeablePrefab.layer;
        bool buildingLarge = (largeStructureLayer.value & (1 << itemLayer)) != 0;

        var obstacles = ghost.GetObstacles(); // 👈 we’ll add this

        foreach (var hit in obstacles)
        {
            if (hit == null) continue;

            if (hit.CompareTag("Player")) continue;

            int hitLayer = hit.gameObject.layer;
            bool hitIsLarge = (largeStructureLayer.value & (1 << hitLayer)) != 0;

            if (!buildingLarge)
            {
                // SMALL ITEM
                if (hitIsLarge)
                    continue; // ✅ allow placing on large

                // ❌ blocked by anything else
                return false;
            }
            else
            {
                // LARGE STRUCTURE
                if (hitIsLarge)
                    return false; // ❌ large cannot overlap large
            }
        }

        return true;
    }

    // ---------------- VISUALS ----------------

    private void UpdatePreviewVisuals(bool blockedByUI)
    {
        if (ghost == null) return;

        if (blockedByUI)
        {
            ghost.SetColor(Color.clear);
        }
        else
        {
            ghost.SetColor(CanPlace() ? Color.green : Color.red);
        }
    }

    // ---------------- RADIUS ----------------

    private void CreateRadiusCircle()
    {
        if (circleObj != null) return;

        circleObj = new GameObject("BuildRadiusCircle");

        circleLine = circleObj.AddComponent<LineRenderer>();
        circleLine.positionCount = CIRCLE_SEGMENTS;
        circleLine.loop = true;
        circleLine.material = new Material(Shader.Find("Sprites/Default"));
        circleLine.startWidth = 0.05f;
        circleLine.endWidth = 0.05f;
        circleLine.useWorldSpace = true;
    }

    private void UpdateRadiusCircle()
    {
        if (circleLine == null || player == null || radiusController == null)
            return;

        float radius = radiusController.GetRadius();
        Vector3 center = player.position;

        if (radius <= 0.01f)
        {
            circleLine.enabled = false;
            return;
        }

        circleLine.enabled = true;

        for (int i = 0; i < CIRCLE_SEGMENTS; i++)
        {
            float angle = (float)i / CIRCLE_SEGMENTS * Mathf.PI * 2f;

            Vector3 pos = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y + Mathf.Sin(angle) * radius,
                -1f
            );

            circleLine.SetPosition(i, pos);
        }
    }
}