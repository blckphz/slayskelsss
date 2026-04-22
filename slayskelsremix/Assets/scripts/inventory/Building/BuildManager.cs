using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Layers")]
    public LayerMask placementMask;         // Must include BOTH Small and Large layers
    public LayerMask interactLayer;        // Check the "Interactable" box in Inspector
    public LayerMask largeStructureLayer;  // Check the "LargeStructure" box in Inspector

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

        // Check if mouse is over UI
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

        // --- IMPROVED LAYER CHECK ---
        int prefabLayer = item.placeablePrefab.layer;
        // This checks if the prefab's layer is included in the interactLayer mask
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

        // VERIFICATION LOG
        Debug.Log($"[BUILD] Selected: {item.itemName} | Layer: {LayerMask.LayerToName(prefabLayer)} | IsSmall: {isCurrentItemSmall}");
    }

    bool CanPlace()
    {
        if (ghost == null) return false;

        List<Collider2D> obstacles = ghost.GetObstacles();

        foreach (var hit in obstacles)
        {
            if (hit == null) continue;
            if (hit.CompareTag("Player")) continue;

            int hitLayer = hit.gameObject.layer;

            // Is the obstacle a Large Structure?
            bool hitLarge = (largeStructureLayer.value & (1 << hitLayer)) != 0;

            // If placing small item on large structure, ignore collision
            if (isCurrentItemSmall && hitLarge)
            {
                continue;
            }

            // Otherwise, it's a real blockage
            return false;
        }

        return true;
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

        GameObject obj = Instantiate(currentItem.placeablePrefab, previewObject.transform.position, Quaternion.identity);

        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);
            BuildingSaveManager.Instance.SaveAfterChange();
        }

        PlayerHotbarManager.Instance.UseSelectedStack(1);
    }

    void Cancel()
    {
        if (previewObject) Destroy(previewObject);
        previewObject = null;
        ghost = null;
        currentItem = null;
        isPlacing = false;
    }

    void UpdateColor(bool blockedUI)
    {
        if (ghost == null) return;

        // FIXED: Removed Color.clear so it doesn't turn into a "shadow"
        if (blockedUI)
        {
            // If over UI, we just make it red or hide it, but not "shadowy"
            ghost.SetColor(new Color(1, 0, 0, 0.2f));
        }
        else
        {
            ghost.SetColor(CanPlace() ? Color.green : Color.red);
        }
    }
}