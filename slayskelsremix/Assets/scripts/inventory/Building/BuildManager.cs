using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Layers")]
    public LayerMask placementMask;
    public LayerMask interactLayer;
    public LayerMask largeStructureLayer;

    [Header("Grid")]
    public float gridSize = 1f;

    private GameObject previewObject;
    private buildSO currentItem;
    private bool isPlacing;
    private ghostBuildPreview ghost;
    private bool isCurrentItemSmall;

    void Update()
    {
        CheckHotbar();

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

        // 1. Instantiate the real object
        GameObject obj = Instantiate(currentItem.placeablePrefab, previewObject.transform.position, Quaternion.identity);

        // 2. Assign Unique ID if it's a table/identifiable object
        tableBehav table = obj.GetComponent<tableBehav>();
        if (table != null)
        {
            table.GenerateUniqueID();
        }

        // 3. Setup BuildIdentity for Save System
        BuildIdentity id = obj.GetComponent<BuildIdentity>() ?? obj.AddComponent<BuildIdentity>();
        id.item = currentItem;

        // 4. Register and Save
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
            bool hitLarge = (largeStructureLayer.value & (1 << hitLayer)) != 0;

            if (isCurrentItemSmall && hitLarge) continue;
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