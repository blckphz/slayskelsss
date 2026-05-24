using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Pathfinding;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("References")]
    public Camera playerCamera;
    public AstarPath astar;

    [Header("Layers")]
    public LayerMask placementMask;
    public LayerMask interactLayer;
    public LayerMask largeStructureLayer;

    [Header("Grid")]
    public float gridSize = 1f;

    [Header("Effects")]
    public GameObject placementEffectPrefab;
    public bool shakeOnPlace = true;
    public float buildShakeIntensity = 0.5f;
    public float buildShakeDuration = 0.1f;

    private GameObject previewObject;
    private buildSO currentItem;
    private ghostBuildPreview ghost;

    private bool isPlacing;
    private bool placementForcedByAbility;
    private ItemData originalToolItem;

    public bool IsPlacing => isPlacing;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (astar != null)
            AstarPath.active.Scan();
    }

    void Update()
    {
        bool restricted = InteractionManager.MasterRestriction;

        if (restricted && !invUIToggle.IsInventoryOpen)
        {
            if (isPlacing)
                Cancel();

            return;
        }

        if (!restricted || invUIToggle.IsInventoryOpen)
            CheckHotbar();

        if (!isPlacing || previewObject == null)
            return;

        MovePreview();

        bool isUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        UpdateColor(isUI);

        if (!isUI &&
            PlayerInputHandler.Instance != null &&
            PlayerInputHandler.Instance.LeftClickPressed())
        {
            TryPlace();
        }
    }

    void CheckHotbar()
    {
        if (PlayerHotbarManager.Instance == null)
            return;

        ItemData item = PlayerHotbarManager.Instance.GetSelectedItem();

        if (placementForcedByAbility && isPlacing)
        {
            if (item != originalToolItem)
                Cancel();

            return;
        }

        if (item != null && item.itemType == ItemType.Constructable)
        {
            buildSO build = item as buildSO;

            if (build == null)
                return;

            if (currentItem == null || currentItem.itemName != build.itemName)
                StartPlacingInternal(build, false);
        }
        else if (isPlacing)
        {
            Cancel();
        }
    }

    public void StartPlacing(buildSO item)
    {
        StartPlacingInternal(item, true);
    }

    private void StartPlacingInternal(buildSO item, bool forced)
    {
        ItemData snapshotTool =
            PlayerHotbarManager.Instance != null
                ? PlayerHotbarManager.Instance.GetSelectedItem()
                : null;

        Cancel();

        currentItem = item;
        isPlacing = true;
        placementForcedByAbility = forced;
        originalToolItem = forced ? snapshotTool : null;

        previewObject = Instantiate(item.placeablePrefab);

        ghost = previewObject.GetComponent<ghostBuildPreview>();

        if (ghost != null)
        {
            ghost.placementMask = placementMask;
            ghost.footprint = item.size;
            ghost.gridSize = gridSize;
            ghost.InitializeGhost();
        }
    }

    void MovePreview()
    {
        Vector2 mouse = PlayerInputHandler.Instance.GetMousePosition();

        Vector3 world = playerCamera.ScreenToWorldPoint(
            new Vector3(mouse.x, mouse.y, Mathf.Abs(playerCamera.transform.position.z))
        );

        float x = Mathf.Floor(world.x / gridSize) * gridSize;
        float y = Mathf.Floor(world.y / gridSize) * gridSize;

        previewObject.transform.position = new Vector3(x, y, 0f);
    }

    void TryPlace()
    {
        if (previewObject == null || currentItem == null)
            return;

        if (!CanPlace())
            return;

        // --- BLOCKING LOGIC ---
        ActionLock.LockThisFrame();
        // ----------------------

        Vector3 pos = previewObject.transform.position;

        GameObject obj = Instantiate(currentItem.placeablePrefab, pos, Quaternion.identity);

        if (shakeOnPlace && CameraShaker.Instance != null)
            CameraShaker.Instance.Shake(buildShakeIntensity, buildShakeDuration);

        if (placementEffectPrefab != null)
        {
            var fx = Instantiate(placementEffectPrefab, pos + Vector3.up * 0.1f, Quaternion.identity);
            Destroy(fx, 2f);
        }

        if (astar != null)
        {
            Bounds b = new Bounds(pos, Vector3.one * Mathf.Max(currentItem.size.x, currentItem.size.y));
            AstarPath.active.UpdateGraphs(b);
        }

        BuildIdentity id = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
        id.item = currentItem;

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);
            BuildingSaveManager.Instance.SaveAfterChange();
        }

        Cancel();
    }

    bool CanPlace()
    {
        if (ghost == null)
            return false;

        List<Collider2D> obstacles = ghost.GetObstacles();

        foreach (var hit in obstacles)
        {
            if (hit == null || hit.CompareTag("Player"))
                continue;

            int layer = hit.gameObject.layer;

            if ((largeStructureLayer.value & (1 << layer)) != 0)
                continue;

            return false;
        }

        return true;
    }

    void UpdateColor(bool blockedUI)
    {
        if (ghost == null) return;

        ghost.SetColor(blockedUI
            ? new Color(1, 0, 0, 0.2f)
            : (CanPlace() ? Color.green : Color.red));
    }

    void Cancel()
    {
        if (previewObject)
            Destroy(previewObject);

        previewObject = null;
        ghost = null;
        currentItem = null;
        isPlacing = false;
        placementForcedByAbility = false;
        originalToolItem = null;
    }
}