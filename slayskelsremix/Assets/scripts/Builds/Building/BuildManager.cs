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
            {
                Debug.Log("[BuildManager] ❌ Cancelled due to MasterRestriction");
                Cancel();
            }
            return;
        }

        if (!restricted || invUIToggle.IsInventoryOpen)
            CheckHotbar();

        if (!isPlacing || previewObject == null)
            return;

        MovePreview();

        bool isUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        UpdateColor(isUI);

        if (!isUI && PlayerInputHandler.Instance != null &&
            PlayerInputHandler.Instance.LeftClickPressed())
        {
            Debug.Log($"[BuildManager] 🖱 Click detected → trying place {currentItem?.itemName}");
            TryPlace();
        }
    }

    // =========================================================
    // HOTBAR CHECK
    // =========================================================
    void CheckHotbar()
    {
        if (PlayerHotbarManager.Instance == null)
            return;

        ItemData item = PlayerHotbarManager.Instance.GetSelectedItem();


        // Ability tool safety
        if (placementForcedByAbility && isPlacing)
        {
            if (item != originalToolItem)
            {
                Debug.Log("[BuildManager] 🔄 Tool changed → cancelling ability placement");
                Cancel();
            }
            return;
        }

        // Only PLACEABLE items
        if (item != null && item.itemType == ItemType.Constructable)
        {
            buildSO build = item as buildSO;

            if (build == null)
            {
                Debug.LogWarning("[BuildManager] ❌ ItemType is Placeable but cast to buildSO failed");
                return;
            }

            if (currentItem == null || currentItem.itemName != build.itemName)
            {
                Debug.Log($"[BuildManager] ✅ Start placing: {build.itemName}");
                StartPlacingInternal(build, false);
            }
        }
        else if (isPlacing)
        {
            Debug.Log("[BuildManager] ❌ Selected item is not placeable → cancel");
            Cancel();
        }
    }

    // =========================================================
    // EXTERNAL CALL
    // =========================================================
    public void StartPlacing(buildSO item)
    {
        Debug.Log($"[BuildManager] 🧰 External placement: {item.name}");
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

        if (item.placeablePrefab == null)
        {
            Debug.LogError($"[BuildManager] ❌ Missing prefab: {item.itemName}");
            return;
        }

        previewObject = Instantiate(item.placeablePrefab);

        ghost = previewObject.GetComponent<ghostBuildPreview>();

        if (ghost != null)
        {
            ghost.placementMask = placementMask;
            ghost.footprint = item.size;
            ghost.gridSize = gridSize;
            ghost.InitializeGhost();

            Debug.Log("[BuildManager] 👻 Ghost initialized");
        }
        else
        {
            Debug.LogWarning("[BuildManager] ⚠ Missing ghostBuildPreview");
        }
    }

    // =========================================================
    // PREVIEW MOVE
    // =========================================================
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

    // =========================================================
    // PLACE
    // =========================================================
    void TryPlace()
    {
        if (previewObject == null || currentItem == null)
        {
            Debug.LogWarning("[BuildManager] ❌ TryPlace null refs");
            return;
        }

        if (!CanPlace())
        {
            Debug.Log("[BuildManager] ❌ Blocked by CanPlace()");
            return;
        }

        Vector3 pos = previewObject.transform.position;

        Debug.Log($"[BuildManager] ✅ PLACING {currentItem.itemName} at {pos}");

        GameObject obj = Instantiate(currentItem.placeablePrefab, pos, Quaternion.identity);

        // FX
        if (shakeOnPlace && CameraShaker.Instance != null)
            CameraShaker.Instance.Shake(buildShakeIntensity, buildShakeDuration);

        if (placementEffectPrefab != null)
        {
            var fx = Instantiate(placementEffectPrefab, pos + Vector3.up * 0.1f, Quaternion.identity);
            Destroy(fx, 2f);
        }

        // Pathfinding update
        if (astar != null)
        {
            Bounds b = new Bounds(pos, Vector3.one * Mathf.Max(currentItem.size.x, currentItem.size.y));
            AstarPath.active.UpdateGraphs(b);
        }

        // Save
        BuildIdentity id = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
        id.item = currentItem;

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);
            BuildingSaveManager.Instance.SaveAfterChange();
        }

        // =====================================================
        // 🔥 ITEM CONSUMPTION (FIXED WITH ITEMTYPE)
        // =====================================================
        if (!placementForcedByAbility)
        {
            var hotbar = PlayerHotbarManager.Instance;

            if (hotbar != null)
            {
                Debug.Log($"[BuildManager] 📦 ItemType = {currentItem.itemType}");

                if (currentItem.itemType == ItemType.Constructable)
                {
                    if (currentItem.consumeAmount > 0)
                    {
                        Debug.Log($"[BuildManager] 🔻 Consuming {currentItem.consumeAmount}");
                        hotbar.UseSelectedStack(currentItem.consumeAmount);
                    }
                    else
                    {
                        Debug.Log("[BuildManager] ♾ Free placement item (consumeAmount = 0)");
                    }
                }
            }
        }

        Cancel();
    }

    // =========================================================
    // VALIDATION
    // =========================================================
    bool CanPlace()
    {
        if (ghost == null)
        {
            Debug.LogWarning("[BuildManager] ❌ Ghost missing");
            return false;
        }

        List<Collider2D> obstacles = ghost.GetObstacles();

        foreach (var hit in obstacles)
        {
            if (hit == null || hit.CompareTag("Player"))
                continue;

            int layer = hit.gameObject.layer;

            if ((largeStructureLayer.value & (1 << layer)) != 0)
            {
                Debug.Log($"[BuildManager] ⚠ Ignoring large structure overlap: {hit.name}");
                continue;
            }

            Debug.Log($"[BuildManager] ❌ Blocked by: {hit.name}");
            return false;
        }

        return true;
    }

    // =========================================================
    // COLOR
    // =========================================================
    void UpdateColor(bool blockedUI)
    {
        if (ghost == null) return;

        ghost.SetColor(blockedUI
            ? new Color(1, 0, 0, 0.2f)
            : (CanPlace() ? Color.green : Color.red));
    }

    // =========================================================
    // CANCEL
    // =========================================================
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

        Debug.Log("[BuildManager] 🧹 Cancelled placement");
    }
}