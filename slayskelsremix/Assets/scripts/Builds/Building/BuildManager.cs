using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Pathfinding;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("References")]
    public Camera playerCamera;
    public AstarPath astar;
    public BuildPlacer placer;

    [Header("Unity Grid")]
    public Grid buildGrid;

    [Header("Base Tilemap")]
    public Tilemap baseTilemap;

    [Header("Layers")]
    public LayerMask placementMask;
    public LayerMask interactLayer;
    public LayerMask largeStructureLayer;
    public LayerMask waterLayer;

    [Header("Effects")]
    public GameObject placementEffectPrefab;
    public bool shakeOnPlace = true;
    public float buildShakeIntensity = 0.5f;
    public float buildShakeDuration = 0.1f;

    private PlayerHotbarManager hotbar;
    private PlayerInputHandler input;

    private bool isPlacing;
    private bool placementForcedByAbility;
    private ItemData originalToolItem;

    public bool IsPlacing => isPlacing;

    public buildSO GetCurrentItem()
    {
        return placer != null ? placer.GetCurrentItem() : null;
    }

    private void Awake()
    {
        Instance = this;

        if (placer == null)
            placer = GetComponent<BuildPlacer>();

        if (placer == null)
            placer = gameObject.AddComponent<BuildPlacer>();

        placer.Initialize(this);
    }

    private void Start()
    {
        hotbar = PlayerHotbarManager.Instance;
        input = PlayerInputHandler.Instance;

        if (placer != null)
            placer.SetInput(input);

        if (astar != null)
            AstarPath.active.Scan();
    }

    private void OnEnable()
    {
        if (PlayerHotbarManager.Instance != null)
        {
            PlayerHotbarManager.Instance.OnSelectedItemChanged +=
                HandleHotbarChanged;
        }

        BuildState.OnBuildModeChanged +=
            HandleBuildModeChanged;
    }

    private void OnDisable()
    {
        if (PlayerHotbarManager.Instance != null)
        {
            PlayerHotbarManager.Instance.OnSelectedItemChanged -=
                HandleHotbarChanged;
        }

        BuildState.OnBuildModeChanged -=
            HandleBuildModeChanged;
    }

    private void Update()
    {
        ActionLock.Tick();

        if (InteractionManager.MasterRestriction &&
            !invUIToggle.IsInventoryOpen)
        {
            if (isPlacing)
                Cancel();

            return;
        }

        if (!isPlacing)
            return;

        if (!placementForcedByAbility)
        {
            ItemData selected =
                hotbar != null
                ? hotbar.GetSelectedItem()
                : null;

            if (!(selected is buildSO))
            {
                Cancel();
                return;
            }
        }

        if (placer != null)
            placer.ProcessPlacement();
    }

    private void HandleHotbarChanged(ItemData item)
    {
        if (placementForcedByAbility &&
            isPlacing)
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
            if (GetCurrentItem() != build)
                StartPlacingInternal(build, false);
        }
        else
        {
            Cancel();
        }
    }

    private void HandleBuildModeChanged(bool enabled)
    {
        if (!enabled)
        {
            Cancel();
            return;
        }

        ItemData selected =
            hotbar != null
            ? hotbar.GetSelectedItem()
            : null;

        if (selected is buildSO build)
        {
            StartPlacingInternal(build, false);
        }
    }

    public void StartPlacing(buildSO item)
    {
        StartPlacingInternal(item, true);
    }

    private void StartPlacingInternal(
        buildSO item,
        bool forced)
    {
        if (item == null || placer == null)
            return;

        ItemData snapshot =
            hotbar != null
            ? hotbar.GetSelectedItem()
            : null;

        Cancel();

        BuildState.SetGridLocked(
            !item.supportsFreePlacement
        );

        isPlacing = true;

        placementForcedByAbility = forced;

        originalToolItem =
            forced ? snapshot : null;

        placer.StartPlacing(item);
    }

    public void Cancel()
    {
        BuildState.SetGridLocked(false);

        if (placer != null)
            placer.Cancel();

        isPlacing = false;
        placementForcedByAbility = false;
        originalToolItem = null;
    }
}