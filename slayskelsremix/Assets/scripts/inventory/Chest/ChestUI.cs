using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestUI : MonoBehaviour
{
    public static ChestUI Instance;

    [Header("UI Structure")]
    [Tooltip("Drag the child GameObject that contains the background/slots here.")]
    public GameObject uiVisualRoot;

    [Header("Slots")]
    public ChestSlotUI[] chestSlots;

    [Header("Inventory Integration")]
    [Tooltip("Drag the object with your invUIToggle script here.")]
    public invUIToggle inventorySystem;

    private ChestInventory currentChest;

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Try to find inventory system if not assigned
        if (inventorySystem == null)
        {
            inventorySystem = Object.FindFirstObjectByType<invUIToggle>();
        }

        // Ensure the UI starts hidden
        if (uiVisualRoot != null)
        {
            uiVisualRoot.SetActive(false);
        }
        else
        {
            Debug.LogError("[ChestUI] ❌ uiVisualRoot is NOT assigned! Drag your 'Panel' into the inspector slot.");
        }
    }

    public void Open(ChestInventory chest)
    {
        currentChest = chest;

        if (uiVisualRoot != null)
        {
            uiVisualRoot.SetActive(true);
        }

        // --- SYNC INVENTORY ---
        if (inventorySystem != null)
        {
            inventorySystem.ForceOpenInventory();
        }

        Refresh();
    }

    public void Close()
    {
        currentChest = null;
        ClearUI();

        if (uiVisualRoot != null)
        {
            uiVisualRoot.SetActive(false);
        }

        // --- SYNC INVENTORY ---
        if (inventorySystem != null)
        {
            inventorySystem.ForceCloseInventory();
        }
    }

    public ChestInventory GetCurrentChest()
    {
        return currentChest;
    }

    /// <summary>
    /// Updates the UI slots to match the currentChest's inventory data.
    /// </summary>
    public void Refresh()
    {
        if (currentChest == null)
        {
            return;
        }

        var items = currentChest.chestItems;

        for (int i = 0; i < chestSlots.Length; i++)
        {
            // If the chest has an item for this slot index, show it.
            if (i < items.Count)
            {
                chestSlots[i].SetSlot(items[i].item, items[i].count);
            }
            // Otherwise, make sure the slot looks empty.
            else
            {
                chestSlots[i].ClearSlot();
            }
        }
    }

    private void ClearUI()
    {
        foreach (var slot in chestSlots)
        {
            if (slot != null) slot.ClearSlot();
        }
    }
}