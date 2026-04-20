using UnityEngine;
using UnityEngine.InputSystem; // Added for Input Action

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("UI Settings")]
    [SerializeField] private GameObject craftingUI; // Assign your UI panel here
    [SerializeField] private InputAction toggleCrafting; // Assign the "ToggleCrafting" action

    private void Awake()
    {
        Instance = this;
    }

    // Enable the input action when the script starts
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

    private void OnToggleCrafting(InputAction.CallbackContext context)
    {
        if (craftingUI != null)
        {
            // Simple toggle: if it's active, turn it off; if off, turn it on.
            craftingUI.SetActive(!craftingUI.activeSelf);
        }
    }

    public void CraftItem(CraftingSO recipe)
    {
        if (recipe == null) return;

        if (recipe.CanCraft())
        {
            // 1. Deduct all ingredients
            foreach (var ingredient in recipe.ingredients)
            {
                InventoryManager.Instance.ConsumeResources(ingredient.item, ingredient.amount);
            }

            // 2. Add the result
            InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultCount);

        }
        else
        {
            Debug.Log("<color=red>[Crafting]</color> Missing required materials.");
        }
    }
}