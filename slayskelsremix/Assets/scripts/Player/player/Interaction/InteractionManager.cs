using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public LayerMask interactLayer;
    public LayerMask largeStructureLayer;
    public Color highlightColor = Color.yellow;

    [Header("Runtime Settings")]
    public bool restrictToInventory = false;

    public static bool MasterRestriction { get; private set; }
    public static bool IsBlockingBuildPreview { get; private set; }

    [Header("Delete Settings")]
    public float deconstructInterval = 0.05f;

    private Camera mainCam;
    private GameObject currentHoverObj;
    private SpriteRenderer currentRenderer;
    private Color originalColor;
    private float deconstructCooldown;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnValidate()
    {
        MasterRestriction = restrictToInventory;
    }

    private void Update()
    {
        if (!BuildState.IsBuildMode)
        {
            ClearHighlight();
            IsBlockingBuildPreview = false;
            return;
        }

        // Disable interaction while placing a building
        if (BuildManager.Instance != null && BuildManager.Instance.IsPlacing)
        {
            ClearHighlight();
            IsBlockingBuildPreview = false;
            return;
        }

        MasterRestriction = restrictToInventory;

        if (MasterRestriction && !invUIToggle.IsInventoryOpen)
        {
            ClearHighlight();
            IsBlockingBuildPreview = false;
            return;
        }

        HandleHover();
        HandleHoldDelete();

        IsBlockingBuildPreview = currentHoverObj != null;
    }

    private Vector2 GetMouseWorldPos()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 world = mainCam.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, Mathf.Abs(mainCam.transform.position.z))
        );

        return new Vector2(world.x, world.y);
    }

    private void HandleHover()
    {
        Vector2 point = GetMouseWorldPos();
        bool isShiftHeld = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        GameObject targetObj = null;

        // Large structures
        if (isShiftHeld)
        {
            Collider2D largeHit = Physics2D.OverlapPoint(point, largeStructureLayer);
            if (largeHit != null) targetObj = largeHit.gameObject;
        }

        // Small objects
        if (targetObj == null)
        {
            Collider2D smallHit = Physics2D.OverlapPoint(point, interactLayer);
            if (smallHit != null) targetObj = smallHit.gameObject;
        }

        if (targetObj != currentHoverObj)
        {
            ClearHighlight();

            if (targetObj != null &&
                (targetObj.GetComponent<objectHealth>() != null ||
                 targetObj.GetComponent<ChestInventory>() != null ||
                 targetObj.GetComponent<BuildIdentity>() != null))
            {
                currentHoverObj = targetObj;
                currentRenderer = targetObj.GetComponent<SpriteRenderer>();

                if (currentRenderer != null)
                {
                    originalColor = currentRenderer.color;
                    currentRenderer.color = highlightColor;
                }

            }
        }

        if (targetObj == null)
            ClearHighlight();
    }

    private void HandleHoldDelete()
    {
        if (!Mouse.current.rightButton.isPressed)
        {
            deconstructCooldown = 0f;
            return;
        }

        deconstructCooldown -= Time.deltaTime;

        if (deconstructCooldown > 0f || currentHoverObj == null)
            return;

        GameObject target = currentHoverObj;

        BuildIdentity build = target.GetComponent<BuildIdentity>();
        if (build != null)
        {
            if (!build.GetsDestroyedByPlayer && build.item != null)
            {
                InventoryManager.Instance.AddItem(build.item, 1);
            }

            if (BuildingSaveManager.Instance != null)
            {
                BuildingSaveManager.Instance.UnregisterBuilding(target);
                BuildingSaveManager.Instance.SaveAfterChange();
            }

            Destroy(target);
            deconstructCooldown = deconstructInterval;

            return;
        }

        objectHealth health = target.GetComponent<objectHealth>();
        if (health != null && health.IsPlayerInRange())
        {
            ChestInventory chest = target.GetComponent<ChestInventory>();
            if (chest != null && !chest.IsEmpty())
                return;

            health.Deconstruct();
            deconstructCooldown = deconstructInterval;

            Debug.Log("[InteractionManager] Object deconstructed");
        }
    }

    private void ClearHighlight()
    {
        if (currentRenderer != null)
            currentRenderer.color = originalColor;

        currentHoverObj = null;
        currentRenderer = null;
    }
}