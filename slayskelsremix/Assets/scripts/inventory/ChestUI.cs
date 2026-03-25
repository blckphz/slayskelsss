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

        // Ensure the UI starts hidden without disabling this script
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
    }

    public ChestInventory GetCurrentChest()
    {
        return currentChest;
    }

    public void Refresh()
    {
        if (currentChest == null)
        {
            Debug.LogWarning("[ChestUI] Refresh called but no chest is assigned!");
            return;
        }


        var items = currentChest.chestItems;

        for (int i = 0; i < chestSlots.Length; i++)
        {
            if (i < items.Count)
            {
                chestSlots[i].SetSlot(items[i].item, items[i].count);
            }
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
            slot.ClearSlot();
        }
    }
}