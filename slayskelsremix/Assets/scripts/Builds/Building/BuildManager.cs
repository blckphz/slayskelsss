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

    [Header("Effects & Shake")]
    public GameObject placementEffectPrefab;
    public bool shakeOnPlace = true;
    public float buildShakeIntensity = 0.5f;
    public float buildShakeDuration = 0.1f;

    private GameObject previewObject;
    private buildSO currentItem;
    private bool isPlacing;
    private ghostBuildPreview ghost;
    private bool isCurrentItemSmall;

    private bool placementForcedByAbility;
    // 🔥 Track the specific tool asset instance that initialized this placement state
    private ItemData originalToolItem;

    void Awake()
    {
        Instance = this;
        Debug.Log("[BuildManager] Instance initialized.");
    }

    void Start()
    {
        if (astar != null)
        {
            Debug.Log("[BuildManager] Scanning pathfinding graphs on startup.");
            AstarPath.active.Scan();
        }
    }

    void Update()
    {
        bool restricted = InteractionManager.MasterRestriction;

        if (restricted && !invUIToggle.IsInventoryOpen)
        {
            if (isPlacing)
            {
                Debug.Log("<color=orange>[BuildManager]</color> Placement canceled due to MasterRestriction.");
                Cancel();
            }
            return;
        }

        if (!restricted || invUIToggle.IsInventoryOpen)
        {
            CheckHotbar();
        }

        if (!isPlacing || previewObject == null)
            return;

        MovePreview();

        bool isUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        UpdateColor(isUI);

        if (!isUI && PlayerInputHandler.Instance != null && PlayerInputHandler.Instance.LeftClickPressed())
        {
            Debug.Log($"[BuildManager] Left-click detected. Attempting placement for: {currentItem?.name}");
            TryPlace();
        }
    }

    void CheckHotbar()
    {
        if (PlayerHotbarManager.Instance == null)
            return;

        ItemData currentHotbarItem = PlayerHotbarManager.Instance.GetSelectedItem();

        // ========================================================
        // STATE CHECK 1: ABILITY DRIVEN PLACEMENT (E.G. SHOVEL)
        // ========================================================
        if (placementForcedByAbility && isPlacing)
        {
            // 🔥 CRITICAL FIX: If the selected hotbar item is no longer the tool that started this, cancel it!
            if (currentHotbarItem != originalToolItem)
            {
                Debug.Log($"[BuildManager] Hotbar selection switched away from original tool ({originalToolItem?.name ?? "None"} -> {currentHotbarItem?.name ?? "None"}). Clearing ghost.");
                Cancel();

                // Don't early return here; allow the code below to see if the NEW item wants to start its own build ghost immediately
            }
            else
            {
                // We are still holding the correct tool, keep holding the ability placement active safely
                return;
            }
        }

        // ========================================================
        // STATE CHECK 2: STANDARD ITEM PLACEMENT (E.G. PLACING A WALL)
        // ========================================================
        if (currentHotbarItem is buildSO build)
        {
            if (currentItem == null || currentItem.itemName != build.itemName)
            {
                Debug.Log($"[BuildManager] Hotbar item match! Starting native item build mode for: {build.name}");
                StartPlacingInternal(build, false);
            }
        }
        else if (isPlacing)
        {
            Debug.Log($"[BuildManager] Selected item is not a buildSO (Currently: {currentHotbarItem?.name ?? "None"}). Canceling placement.");
            Cancel();
        }
    }

    // Public method exposed to the Shovel/Abilities
    public void StartPlacing(buildSO item)
    {
        Debug.Log($"<color=cyan>[BuildManager]</color> StartPlacing externally triggered by an Ability for item: {item.name}");
        StartPlacingInternal(item, true);
    }

    private void StartPlacingInternal(buildSO item, bool forcedByAbility)
    {
        // Capture what tool was actively highlighted before wiping clean layouts
        ItemData trackingTool = PlayerHotbarManager.Instance != null ? PlayerHotbarManager.Instance.GetSelectedItem() : null;

        Cancel();

        currentItem = item;
        isPlacing = true;
        placementForcedByAbility = forcedByAbility;
        originalToolItem = forcedByAbility ? trackingTool : null;

        if (item.placeablePrefab == null)
        {
            Debug.LogError($"[BuildManager CRITICAL] The buildSO '{item.name}' does not have a placeablePrefab assigned!");
            return;
        }

        int prefabLayer = item.placeablePrefab.layer;
        isCurrentItemSmall = (interactLayer.value & (1 << prefabLayer)) != 0;

        Debug.Log($"[BuildManager] Instantiating preview object for layer validation. Small structure: {isCurrentItemSmall}");
        previewObject = Instantiate(item.placeablePrefab);

        ghost = previewObject.GetComponent<ghostBuildPreview>();

        if (ghost != null)
        {
            ghost.placementMask = placementMask;
            ghost.footprint = currentItem.size;
            ghost.gridSize = gridSize;
            ghost.InitializeGhost();
            Debug.Log("[BuildManager] ghostBuildPreview initialization successful.");
        }
        else
        {
            Debug.LogWarning($"[BuildManager WARNING] Spawned prefab '{previewObject.name}' does not contain a ghostBuildPreview script attached.");
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
        if (previewObject == null)
        {
            Debug.LogWarning("[BuildManager] TryPlace failed: previewObject is null.");
            return;
        }

        if (!CanPlace())
        {
            Debug.LogWarning($"[BuildManager] TryPlace blocked: Obstacles or invalid rules detected by Ghost for position {previewObject.transform.position}");
            return;
        }

        Vector3 placePos = previewObject.transform.position;
        Debug.Log($"<color=green>[BuildManager SUCCESS]</color> Rules passed. Spawning placement instance: {currentItem.placeablePrefab.name} at {placePos}");

        GameObject obj = Instantiate(currentItem.placeablePrefab, placePos, Quaternion.identity);

        if (shakeOnPlace && CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(buildShakeIntensity, buildShakeDuration);
        }

        if (currentItem.placementSound != null)
        {
            AudioSource.PlayClipAtPoint(currentItem.placementSound, placePos);
        }

        if (placementEffectPrefab != null)
        {
            GameObject fxObj = Instantiate(placementEffectPrefab, placePos + Vector3.up * 0.1f, Quaternion.identity);
            Destroy(fxObj, 2f);
        }

        if (astar != null)
        {
            Bounds bounds = new Bounds(placePos, Vector3.one * Mathf.Max(currentItem.size.x, currentItem.size.y));
            Debug.Log("[BuildManager] Syncing pathfinding nodes to structural changes.");
            AstarPath.active.UpdateGraphs(bounds);
        }

        tableBehav table = obj.GetComponent<tableBehav>();
        if (table != null) table.GenerateUniqueID();

        BuildIdentity id = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
        id.item = currentItem;

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);
            BuildingSaveManager.Instance.SaveAfterChange();
        }

        if (!placementForcedByAbility)
        {
            Debug.Log($"[BuildManager] Consuming stock from direct hotbar item. Count: {currentItem.consumeAmount}");
            PlayerHotbarManager.Instance.UseSelectedStack(currentItem.consumeAmount);
        }
        else
        {
            Debug.Log("[BuildManager] Placement authorized by specialized tool ability. Inventory stock consumption skipped.");
        }

        Cancel();
    }

    bool CanPlace()
    {
        if (ghost == null)
        {
            Debug.LogWarning("[BuildManager] CanPlace returning false because 'ghost' component reference is missing.");
            return false;
        }

        List<Collider2D> obstacles = ghost.GetObstacles();
        Debug.Log($"[BuildManager Check] Checking collision metrics. Obstacles count: {obstacles.Count}");

        foreach (var hit in obstacles)
        {
            if (hit == null || hit.CompareTag("Player"))
                continue;

            int hitLayer = hit.gameObject.layer;

            if (isCurrentItemSmall && (largeStructureLayer.value & (1 << hitLayer)) != 0)
            {
                Debug.Log($"[BuildManager Check] Small item overlapping large structure layer ({LayerMask.LayerToName(hitLayer)}). Allowing overlay.");
                continue;
            }

            Debug.Log($"[BuildManager Check] Placement blocked by overlapping object: {hit.gameObject.name} on layer: {LayerMask.LayerToName(hitLayer)}");
            return false;
        }

        return true;
    }

    void UpdateColor(bool blockedUI)
    {
        if (ghost == null)
            return;

        if (blockedUI)
            ghost.SetColor(new Color(1, 0, 0, 0.2f));
        else
            ghost.SetColor(CanPlace() ? Color.green : Color.red);
    }

    void Cancel()
    {
        if (previewObject)
        {
            Debug.Log($"[BuildManager] Destroying build preview object for: {currentItem?.name ?? "Unknown"}");
            Destroy(previewObject);
        }

        previewObject = null;
        ghost = null;
        currentItem = null;
        isPlacing = false;
        placementForcedByAbility = false;
        originalToolItem = null; // Clean instance variable references safely
    }
}