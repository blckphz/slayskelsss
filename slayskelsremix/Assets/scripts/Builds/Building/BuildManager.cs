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

    void OnEnable()
    {
        if (PlayerHotbarManager.Instance != null)
            PlayerHotbarManager.Instance.OnSelectedItemChanged += HandleHotbarChanged;

        BuildState.OnBuildModeChanged += HandleBuildModeChanged;
    }

    void OnDisable()
    {
        if (PlayerHotbarManager.Instance != null)
            PlayerHotbarManager.Instance.OnSelectedItemChanged -= HandleHotbarChanged;

        BuildState.OnBuildModeChanged -= HandleBuildModeChanged;
    }

    void Update()
    {
        ActionLock.Tick();

        bool restricted = InteractionManager.MasterRestriction;

        if (restricted && !invUIToggle.IsInventoryOpen)
        {
            if (isPlacing)
                Cancel();

            return;
        }

        if (!isPlacing || previewObject == null)
            return;

        ItemData selectedItem = PlayerHotbarManager.Instance != null
            ? PlayerHotbarManager.Instance.GetSelectedItem()
            : null;

        if (!(selectedItem is buildSO))
        {
            Cancel();
            return;
        }

        MovePreview();

        bool isUI = EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject();

        UpdateColor(isUI);

        if (!isUI &&
            PlayerInputHandler.Instance != null &&
            PlayerInputHandler.Instance.LeftClickPressed())
        {
            TryPlace();
        }
    }

    void HandleHotbarChanged(ItemData item)
    {
        if (placementForcedByAbility && isPlacing)
        {
            if (item != originalToolItem)
                Cancel();

            return;
        }

        if (!BuildState.IsBuildMode)
        {
            Cancel();
            return;
        }

        if (item is buildSO build)
        {
            if (currentItem == null || currentItem != build)
                StartPlacingInternal(build, false);
        }
        else
        {
            Cancel();
        }
    }

    void HandleBuildModeChanged(bool enabled)
    {
        ItemData selected =
            PlayerHotbarManager.Instance != null
                ? PlayerHotbarManager.Instance.GetSelectedItem()
                : null;

        if (!enabled)
        {
            Cancel();
            return;
        }

        if (selected is buildSO build)
            StartPlacingInternal(build, false);
    }

    public void StartPlacing(buildSO item)
    {
        StartPlacingInternal(item, true);
    }

    void StartPlacingInternal(buildSO item, bool forced)
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
            new Vector3(
                mouse.x,
                mouse.y,
                Mathf.Abs(playerCamera.transform.position.z)
            )
        );

        if (BuildState.UseGridPlacement)
        {
            float x = Mathf.Floor(world.x / gridSize) * gridSize;
            float y = Mathf.Floor(world.y / gridSize) * gridSize;

            previewObject.transform.position =
                new Vector3(x, y, 0f);
        }
        else
        {
            previewObject.transform.position =
                new Vector3(world.x, world.y, 0f);
        }
    }

    void TryPlace()
    {
        if (previewObject == null || currentItem == null)
            return;

        if (!CanPlace())
        {
            InteractionUI.Instance?.Show("Can't place there");
            return;
        }

        Debug.Log($"[BUILD] Trying to place: {currentItem.itemName}");

        ItemData selected =
            PlayerHotbarManager.Instance.GetSelectedItem();

        Debug.Log($"[BUILD] Selected hotbar item: {(selected != null ? selected.itemName : "NULL")}");

        Vector3 pos = previewObject.transform.position;

        GameObject obj = Instantiate(
            currentItem.placeablePrefab,
            pos,
            Quaternion.identity
        );

        Debug.Log($"[BUILD] Spawned prefab: {obj.name}");

        PlayerHotbarManager.Instance.UseSelectedStack(1);

        Debug.Log("[BUILD] Removed 1 item from hotbar");

        if (shakeOnPlace && CameraShaker.Instance != null)
            CameraShaker.Instance.Shake(
                buildShakeIntensity,
                buildShakeDuration
            );

        if (placementEffectPrefab != null)
        {
            var fx = Instantiate(
                placementEffectPrefab,
                pos + Vector3.up * 0.1f,
                Quaternion.identity
            );

            Destroy(fx, 2f);
        }

        if (astar != null)
        {
            float size =
                Mathf.Max(currentItem.size.x, currentItem.size.y);

            Bounds b =
                new Bounds(pos, Vector3.one * size);

            AstarPath.active.UpdateGraphs(b);
        }

        BuildIdentity id = obj.GetComponent<BuildIdentity>();

        if (id == null)
            id = obj.AddComponent<BuildIdentity>();

        id.item = currentItem;

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);
            BuildingSaveManager.Instance.SaveAfterChange();
        }

        Debug.Log("[BUILD] Placement complete");
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
        if (ghost == null)
            return;

        ghost.SetColor(
            blockedUI
                ? new Color(1, 0, 0, 0.2f)
                : (CanPlace() ? Color.green : Color.red)
        );
    }

    public void Cancel()
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