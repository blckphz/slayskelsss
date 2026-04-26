using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CraftUIManager : MonoBehaviour
{
    public static CraftUIManager Instance;

    [Header("UI Grids")]
    public Transform bgGridParent;
    public Transform iconGridParent;

    [Header("Visual Settings")]
    public float normalAlpha = 0.5f;
    public float selectedAlpha = 1.0f;

    private List<Transform> bgSlots = new List<Transform>();
    private List<Transform> iconSlots = new List<Transform>();
    private List<CraftingSO> filteredRecipes = new List<CraftingSO>();

    private void Awake()
    {
        Instance = this;
        foreach (Transform child in bgGridParent) bgSlots.Add(child);
        foreach (Transform child in iconGridParent) iconSlots.Add(child);
    }

    public void RefreshGrid()
    {
        // FIXED: Now looking at CraftingManager.Instance.knownRecipes
        if (CraftingManager.Instance == null) return;

        filteredRecipes = CraftingManager.Instance.knownRecipes;

        for (int i = 0; i < bgSlots.Count; i++)
        {
            if (i < filteredRecipes.Count)
            {
                bgSlots[i].gameObject.SetActive(true);
                iconSlots[i].gameObject.SetActive(true);
                Image icon = iconSlots[i].GetComponent<Image>();
                if (icon != null) icon.sprite = filteredRecipes[i].resultItem.icon;
            }
            else
            {
                bgSlots[i].gameObject.SetActive(false);
                iconSlots[i].gameObject.SetActive(false);
            }
        }
    }
}