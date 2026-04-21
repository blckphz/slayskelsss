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
    public BuildRadiusController radiusController;
    public InventoryManager inventory;

    [Header("Grid")]
    public float gridSize = 1f;

    private GameObject previewObject;
    private buildSO currentItem;
    private bool isPlacing = false;

    private GameObject circleObj;
    private LineRenderer circleLine;

    private const int CIRCLE_SEGMENTS = 64;

    private void Start()
    {
        Debug.Log("[BuildManager] Initialized");
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

    // ---------------- START / CANCEL ----------------

    public void StartPlacing(buildSO item)
    {
        Cancel();

        if (item == null || item.placeablePrefab == null) return;

        currentItem = item;
        isPlacing = true;

        previewObject = Instantiate(item.placeablePrefab);

        CreateRadiusCircle();

        ghostBuildPreview ghost = previewObject.GetComponent<ghostBuildPreview>();
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
        isPlacing = false;
    }

    // ---------------- MOVE PREVIEW ----------------

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

        BuildIdentity identity = obj.GetComponent<BuildIdentity>() ??
                                 obj.AddComponent<BuildIdentity>();

        identity.item = currentItem;

        PlayerHotbarManager.Instance.UseSelectedStack(1);
    }

    // ---------------- 🔥 SPRITE FOOTPRINT BLOCKING (FIXED) ----------------

    private bool CanPlace()
    {
        if (previewObject == null) return false;

        Vector3 pos = previewObject.transform.position;

        // 1. Radius check
        if (radiusController != null &&
            !radiusController.IsWithinRadius(pos))
        {
            Debug.Log("[Build] ❌ Outside radius");
            return false;
        }

        // 2. REAL FOOTPRINT CHECK (SPRITE / COLLIDER SIZE)
        Collider2D previewCol = previewObject.GetComponent<Collider2D>();

        if (previewCol == null)
        {
            Debug.LogWarning("[Build] ❌ No Collider2D on prefab!");
            return false;
        }

        Vector2 size = previewCol.bounds.size;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            pos,
            size,
            0f,
            placementMask
        );

        Debug.Log($"[Build] Checking {pos} | Size {size} | Hits {hits.Length}");

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            if (hit.gameObject == previewObject)
                continue;

            if (hit.GetComponent<ghostBuildPreview>() != null)
                continue;

            Debug.Log($"[Build] ❌ BLOCKED BY: {hit.name}");
            return false;
        }

        Debug.Log("[Build] ✅ Placement allowed");
        return true;
    }

    // ---------------- VISUALS ----------------

    private void UpdatePreviewVisuals(bool blockedByUI)
    {
        if (previewObject == null) return;

        ghostBuildPreview ghost = previewObject.GetComponent<ghostBuildPreview>();
        if (ghost == null) return;

        ghost.SetColor(blockedByUI ? Color.clear :
            (CanPlace() ? Color.green : Color.red));
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