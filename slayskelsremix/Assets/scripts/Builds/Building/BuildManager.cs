using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Pathfinding;

public class BuildManager : MonoBehaviour
{
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

    private GameObject previewObject;
    private buildSO currentItem;
    private bool isPlacing;
    private ghostBuildPreview ghost;
    private bool isCurrentItemSmall;

    void Start() { if (astar != null) AstarPath.active.Scan(); }

    void Update()
    {
        // Look at the Master Restriction set in the InteractionManager
        bool restricted = InteractionManager.MasterRestriction;

        if (restricted && !invUIToggle.IsInventoryOpen)
        {
            if (isPlacing) Cancel();
            return;
        }

        // Only allow switching items if not restricted or if inventory is open
        if (!restricted || invUIToggle.IsInventoryOpen)
        {
            CheckHotbar();
        }

        if (!isPlacing || previewObject == null) return;

        MovePreview();

        bool isUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        UpdateColor(isUI);

        if (!isUI && PlayerInputHandler.Instance != null && PlayerInputHandler.Instance.LeftClickPressed())
        {
            TryPlace();
        }
    }

    void CheckHotbar()
    {
        if (PlayerHotbarManager.Instance == null) return;
        ItemData item = PlayerHotbarManager.Instance.GetSelectedItem();

        if (item is buildSO build)
        {
            if (currentItem == null || currentItem.itemName != build.itemName)
                StartPlacing(build);
        }
        else if (isPlacing)
        {
            Cancel();
        }
    }

    public void StartPlacing(buildSO item)
    {
        Cancel();
        currentItem = item;
        isPlacing = true;
        int prefabLayer = item.placeablePrefab.layer;
        isCurrentItemSmall = (interactLayer.value & (1 << prefabLayer)) != 0;
        previewObject = Instantiate(item.placeablePrefab);
        ghost = previewObject.GetComponent<ghostBuildPreview>();

        if (ghost != null)
        {
            ghost.placementMask = placementMask;
            ghost.footprint = currentItem.size;
            ghost.gridSize = gridSize;
            ghost.InitializeGhost();
        }
    }

    void MovePreview()
    {
        Vector2 mouse = PlayerInputHandler.Instance.GetMousePosition();
        Vector3 world = playerCamera.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, Mathf.Abs(playerCamera.transform.position.z)));
        float x = Mathf.Floor(world.x / gridSize) * gridSize;
        float y = Mathf.Floor(world.y / gridSize) * gridSize;
        previewObject.transform.position = new Vector3(x, y, 0f);
    }

    void TryPlace()
    {
        if (previewObject == null || !CanPlace()) return;
        Vector3 placePos = previewObject.transform.position;
        GameObject obj = Instantiate(currentItem.placeablePrefab, placePos, Quaternion.identity);

        if (currentItem.placementSound != null) AudioSource.PlayClipAtPoint(currentItem.placementSound, placePos);
        if (placementEffectPrefab != null)
        {
            GameObject fxObj = Instantiate(placementEffectPrefab, placePos + Vector3.up * 0.1f, Quaternion.identity);
            Destroy(fxObj, 2f);
        }

        if (astar != null)
        {
            Bounds bounds = new Bounds(placePos, Vector3.one * Mathf.Max(currentItem.size.x, currentItem.size.y));
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
        PlayerHotbarManager.Instance.UseSelectedStack(1);
    }

    bool CanPlace()
    {
        if (ghost == null) return false;
        List<Collider2D> obstacles = ghost.GetObstacles();
        foreach (var hit in obstacles)
        {
            if (hit == null || hit.CompareTag("Player")) continue;
            int hitLayer = hit.gameObject.layer;
            if (isCurrentItemSmall && (largeStructureLayer.value & (1 << hitLayer)) != 0) continue;
            return false;
        }
        return true;
    }

    void UpdateColor(bool blockedUI)
    {
        if (ghost == null) return;
        if (blockedUI) ghost.SetColor(new Color(1, 0, 0, 0.2f));
        else ghost.SetColor(CanPlace() ? Color.green : Color.red);
    }

    void Cancel()
    {
        if (previewObject) Destroy(previewObject);
        previewObject = null;
        ghost = null;
        currentItem = null;
        isPlacing = false;
    }
}