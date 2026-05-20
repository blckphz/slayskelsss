using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CampfireSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{



    [Header("References")]
    public CampfireBehav campfire;

    [Header("UI")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private TextMeshProUGUI amountText;


    public void Start()
    {
        campfire = FindAnyObjectByType<CampfireBehav>();
    }



    // =========================
    // LEFT CLICK REMOVE FUEL
    // =========================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (campfire == null)
        {
            Debug.LogError("[CampfireUI] Campfire is NULL on click!");
            return;
        }

        if (campfire.fuelItem == null)
        {
            Debug.LogWarning("[CampfireUI] fuelItem is NULL on click!");
            return;
        }

        if (campfire.fuelAmount < 1f)
        {
            Debug.Log("[CampfireUI] No fuel to remove.");
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int removed = campfire.RemoveFuel(1);

            Debug.Log($"[CampfireUI] Removed {removed} fuel");

            if (removed > 0)
            {
                InventoryManager.Instance.AddItem(campfire.fuelItem, removed);

                PlayerHotbarManager.Instance?.SyncHotbarToData();
                InventoryManager.Instance.SaveInventory();
                BuildingSaveManager.Instance?.SaveNow();

                UpdateUI();
                InvUI.Instance?.RefreshUI();
            }
        }
    }

    // =========================
    // DROP INTO CAMPFIRE
    // =========================
    public void OnDrop(PointerEventData eventData)
    {
        if (campfire == null)
        {
            Debug.LogError("[CampfireUI] Campfire is NULL on drop!");
            return;
        }

        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot == null)
        {
            Debug.LogWarning("[CampfireUI] Dragged slot is NULL");
            return;
        }

        ItemData item = draggedSlot.GetItem();
        if (item == null)
        {
            Debug.LogWarning("[CampfireUI] Dropped item is NULL");
            return;
        }

        if (item.itemID != campfire.fuelItem.itemID)
        {
            Debug.Log("[CampfireUI] Wrong item dropped into campfire");
            return;
        }

        int added = campfire.AddFuel(item, draggedSlot.GetCount());

        Debug.Log($"[CampfireUI] Added fuel: {added}");

        if (added > 0)
        {
            InventoryManager.Instance.RemoveItem(item, added);

            if (!campfire.isBurning)
                campfire.Ignite();

            PlayerHotbarManager.Instance?.SyncHotbarToData();
            InventoryManager.Instance.SaveInventory();
            BuildingSaveManager.Instance?.SaveNow();

            UpdateUI();
            InvUI.Instance?.RefreshUI();
        }
    }

    // =========================
    // UI UPDATE
    // =========================
    public void UpdateUI()
    {
        if (campfire == null)
        {
            Debug.LogError("[CampfireUI] UpdateUI aborted: campfire NULL");
            ResetSlot();
            return;
        }

        Debug.Log($"[CampfireUI] UpdateUI → Fuel: {campfire.fuelAmount}, Item: {(campfire.fuelItem ? campfire.fuelItem.name : "NULL")}");

        if (campfire.fuelAmount <= 0)
        {
            Debug.Log("[CampfireUI] Fuel is 0 → Reset UI");
            ResetSlot();
            return;
        }

        if (campfire.fuelItem == null)
        {
            Debug.LogError("[CampfireUI] fuelItem is NULL but fuel exists!");
            ResetSlot();
            return;
        }

        if (itemIconImage != null)
        {
            itemIconImage.sprite = campfire.fuelItem.icon;
            itemIconImage.color = Color.white;
            itemIconImage.enabled = true;
        }

        if (amountText != null)
        {
            int displayFuel = Mathf.CeilToInt(campfire.fuelAmount);
            amountText.gameObject.SetActive(true);
            amountText.text = displayFuel.ToString();

            Debug.Log($"[CampfireUI] Display Fuel: {displayFuel}");
        }
    }

    // =========================
    // RESET SLOT
    // =========================
    public void ResetSlot()
    {
        Debug.Log("[CampfireUI] ResetSlot called");

        if (itemIconImage != null)
        {
            itemIconImage.sprite = emptySprite;
            itemIconImage.color = emptySprite != null ? Color.white : new Color(1, 1, 1, 0);
        }

        if (amountText != null)
            amountText.gameObject.SetActive(false);
    }

    // =========================
    // HIDE
    // =========================
    public void HideOutsideRange()
    {
        if (itemIconImage != null)
            itemIconImage.color = new Color(1, 1, 1, 0);

        if (amountText != null)
            amountText.gameObject.SetActive(false);
    }
}