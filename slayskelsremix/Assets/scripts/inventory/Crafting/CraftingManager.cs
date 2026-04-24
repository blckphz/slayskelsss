using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("UI Settings")]
    [SerializeField] private GameObject craftingUI;
    [SerializeField] private InputAction toggleCrafting;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        toggleCrafting.Enable();
        toggleCrafting.performed += OnToggleCrafting;
    }

    private void OnDisable()
    {
        toggleCrafting.Disable();
        toggleCrafting.performed -= OnToggleCrafting;
    }

    // This handles the Input System (Keyboard/Controller)
    private void OnToggleCrafting(InputAction.CallbackContext context)
    {
        ToggleCraftingUI();
    }

    // Public method for the UI Button to call
    public void ToggleCraftingUI()
    {
        if (craftingUI != null)
        {
            bool isActive = !craftingUI.activeSelf;
            craftingUI.SetActive(isActive);

            // Optional: Handle Mouse Cursor visibility
            if (isActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Refresh the UI when opened to show latest inventory counts
                if (CraftUIManager.Instance != null) CraftUIManager.Instance.RefreshGrid();
            }
            else
            {
                // Cursor.lockState = CursorLockMode.Locked; // Uncomment if your game is First Person
                // Cursor.visible = false;
            }
        }
    }

    public void CraftItem(CraftingSO recipe)
    {
        if (recipe == null) return;

        if (recipe.CanCraft())
        {
            foreach (var ingredient in recipe.ingredients)
            {
                InventoryManager.Instance.RemoveItem(ingredient.item, ingredient.amount);
            }
            InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultCount);
        }
        else
        {
            Debug.Log("<color=red>[Crafting]</color> Missing required materials.");
        }
    }
}