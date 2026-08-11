using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Pathfinding;


public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;


    [Header("References")]
    public Camera playerCamera;
    public AstarPath astar;


    [Header("Unity Grid")]
    public Grid buildGrid;


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



    private GameObject previewObject;

    private buildSO currentItem;

    private ghostBuildPreview ghost;

    private RotateBuildables previewRotation;



    private PlayerHotbarManager hotbar;

    private PlayerInputHandler input;



    private bool isPlacing;

    private bool placementForcedByAbility;

    private ItemData originalToolItem;



    private bool lastCanPlace;



    public bool IsPlacing => isPlacing;


    public buildSO GetCurrentItem()
    {
        return currentItem;
    }




    // ===============================
    // UNITY
    // ===============================

    private void Awake()
    {
        Instance = this;
    }



    private void Start()
    {
        hotbar =
            PlayerHotbarManager.Instance;


        input =
            PlayerInputHandler.Instance;



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





    // ===============================
    // UPDATE
    // ===============================

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



        if (!isPlacing ||
            previewObject == null)
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




        MovePreview();



        if (Keyboard.current != null &&
     Keyboard.current.rKey.wasPressedThisFrame)
        {
            previewRotation?.ToggleSprite();
        }




        bool isUI =
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();




        lastCanPlace =
            !isUI && CanPlace();



        UpdateColor(
            isUI,
            lastCanPlace
        );



        if (lastCanPlace &&
            input != null &&
            input.LeftClickPressed())
        {
            TryPlace();
        }
    }





    // ===============================
    // HOTBAR
    // ===============================

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
            if (currentItem != build)
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
            StartPlacingInternal(
                build,
                false
            );
        }
    }// ===============================
     // START PLACING
     // ===============================

    public void StartPlacing(buildSO item)
    {
        StartPlacingInternal(item, true);
    }



    private void StartPlacingInternal(
        buildSO item,
        bool forced)
    {
        ItemData snapshot =
            hotbar != null
            ? hotbar.GetSelectedItem()
            : null;



        Cancel();



        currentItem = item;



        BuildState.SetGridLocked(
            !item.supportsFreePlacement
        );



        isPlacing = true;


        placementForcedByAbility = forced;


        originalToolItem =
            forced ? snapshot : null;




        previewObject =
            Instantiate(
                item.placeablePrefab
            );



        ghost =
            previewObject.GetComponent<ghostBuildPreview>();


        previewRotation =
            previewObject.GetComponent<RotateBuildables>();




        if (ghost != null)
        {
            ghost.placementMask =
                placementMask;


            ghost.footprint =
                item.size;


            ghost.grid =
                buildGrid;



            ghost.InitializeGhost();
        }
    }






    // ===============================
    // MOVE PREVIEW
    // ===============================

    private void MovePreview()
    {
        if (input == null ||
            previewObject == null)
            return;



        Vector2 mouse =
            input.GetMousePosition();




        Vector3 world =
            playerCamera.ScreenToWorldPoint(
                new Vector3(
                    mouse.x,
                    mouse.y,
                    Mathf.Abs(
                        playerCamera.transform.position.z
                    )
                )
            );



        Vector3 position;



        if (BuildState.UseGridPlacement &&
            buildGrid != null)
        {
            Vector3Int cell =
                buildGrid.WorldToCell(world);



            position =
                buildGrid.GetCellCenterWorld(cell);
        }
        else
        {
            position =
                new Vector3(
                    world.x,
                    world.y,
                    0f
                );
        }




        previewObject.transform.position =
            position;
    }







    // ===============================
    // PLACE
    // ===============================

    private void TryPlace()
    {
        if (previewObject == null ||
            currentItem == null)
            return;



        Vector3 position =
            previewObject.transform.position;



        Vector3Int cell =
            buildGrid != null
            ? buildGrid.WorldToCell(position)
            : Vector3Int.zero;




        GameObject obj =
            Instantiate(
                currentItem.placeablePrefab,
                position,
                Quaternion.identity
            );





        RotateBuildables placedRotation =
            obj.GetComponent<RotateBuildables>();



        if (previewRotation != null &&
            placedRotation != null)
        {
            placedRotation.ApplyState(
                previewRotation.UsingSecondSprite
            );
        }





        BuildIdentity identity =
            obj.GetComponent<BuildIdentity>();



        if (identity == null)
            identity =
                obj.AddComponent<BuildIdentity>();



        identity.item =
            currentItem;


        identity.cell =
            cell;





        SoilOccupancyManager.Instance?.Register(
            cell,
            identity
        );





        tileReplaceManager.Instance?.ReplaceTile(
            position,
            currentItem
        );





        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.RegisterBuilding(obj);

            BuildingSaveManager.Instance.SaveAfterChange();
        }




        UpdatePathfinding(position);




        PlayPlacementEffects(position);





        if (currentItem.placementSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(
                currentItem.placementSound
            );
        }





        if (hotbar != null)
        {
            hotbar.UseSelectedStack(
                currentItem.consumeAmount
            );
        }
    }






    // ===============================
    // PATHFINDING
    // ===============================

    private void UpdatePathfinding(
        Vector3 position)
    {
        if (astar == null)
            return;



        float size =
            Mathf.Max(
                currentItem.size.x,
                currentItem.size.y
            );



        Bounds bounds =
            new Bounds(
                position,
                Vector3.one * size
            );



        AstarPath.active.UpdateGraphs(
            bounds
        );
    }






    // ===============================
    // EFFECTS
    // ===============================

    private void PlayPlacementEffects(
        Vector3 position)
    {
        if (shakeOnPlace &&
            CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(
                buildShakeIntensity,
                buildShakeDuration
            );
        }




        if (placementEffectPrefab != null)
        {
            GameObject fx =
                Instantiate(
                    placementEffectPrefab,
                    position + Vector3.up * 0.1f,
                    Quaternion.identity
                );


            Destroy(
                fx,
                2f
            );
        }
    }






    // ===============================
    // VALIDATION
    // ===============================

    private bool CanPlace()
    {
        if (ghost == null)
            return false;


        // ===============================
        // WATER CHECK
        // ===============================

        Collider2D water =
            Physics2D.OverlapPoint(
                previewObject.transform.position,
                waterLayer
            );


        // If on water, only allow baseTile buildings
        if (water != null && !currentItem.baseTile)
        {
            return false;
        }



        // ===============================
        // NORMAL OBSTACLE CHECK
        // ===============================

        foreach (Collider2D hit in ghost.GetObstacles())
        {
            if (hit == null)
                continue;



            int layer =
                hit.gameObject.layer;



            if (hit.CompareTag("Player"))
                continue;



            if ((interactLayer.value &
                (1 << layer)) != 0)
            {
                continue;
            }



            if ((largeStructureLayer.value &
                (1 << layer)) != 0)
            {
                continue;
            }



            return false;
        }



        return true;
    }







    // ===============================
    // COLOR
    // ===============================

    private void UpdateColor(
        bool blockedUI,
        bool canPlace)
    {
        if (ghost == null)
            return;



        if (blockedUI)
        {
            ghost.SetColor(
                new Color(
                    1f,
                    0f,
                    0f,
                    0.2f
                )
            );

            return;
        }




        ghost.SetColor(
            canPlace
            ? Color.green
            : Color.red
        );
    }






    // ===============================
    // CANCEL
    // ===============================

    public void Cancel()
    {
        BuildState.SetGridLocked(false);



        if (previewObject != null)
            Destroy(previewObject);



        previewObject = null;


        ghost = null;


        previewRotation = null;


        currentItem = null;



        isPlacing = false;


        placementForcedByAbility = false;


        originalToolItem = null;
    }
}