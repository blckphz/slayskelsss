using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CampfireSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("References")]
    public CampfireBehav campfire;
    public int woodID;

    [Header("UI")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private TextMeshProUGUI amountText;

    // LEFT CLICK TO REMOVE WOOD
    public void OnPointerClick(PointerEventData eventData)
    {
        if (campfire == null || campfire.fuelItem == null || campfire.fuelAmount < 1f)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int removed = campfire.RemoveFuel(1);
            if (removed > 0)
            {
                InventoryManager.Instance.AddItem(campfire.fuelItem, removed);
                UpdateUI();
                InvUI.Instance?.RefreshUI();
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot == null) return;

        ItemData item = draggedSlot.GetItem();
        if (item == null || item.itemID != woodID || campfire == null) return;

        int added = campfire.AddFuel(item, draggedSlot.GetCount());
        if (added > 0)
        {
            InventoryManager.Instance.RemoveItem(item, added);
            if (!campfire.isBurning) campfire.Ignite();
            UpdateUI();
            InvUI.Instance?.RefreshUI();
            BuildingSaveManager.Instance?.SaveNow();
        }
    }

    public void UpdateUI()
    {
        if (campfire == null || campfire.fuelAmount <= 0)
        {
            ResetSlot();
            return;
        }

        if (itemIconImage != null)
        {
            itemIconImage.sprite = campfire.fuelItem.icon;
            itemIconImage.color = new Color(1, 1, 1, 1); // Opacity 100%
            itemIconImage.enabled = true;
        }

        if (amountText != null)
        {
            int displayFuel = Mathf.CeilToInt(campfire.fuelAmount);
            amountText.gameObject.SetActive(displayFuel > 0);
            amountText.text = displayFuel.ToString();
        }
    }

    public void ResetSlot()
    {
        if (itemIconImage != null)
        {
            itemIconImage.sprite = emptySprite;
            // If no item and no empty sprite, hide it (Opacity 0)
            itemIconImage.color = emptySprite != null ? Color.white : new Color(1, 1, 1, 0);
        }

        if (amountText != null)
            amountText.gameObject.SetActive(false);
    }

    public void HideOutsideRange()
    {
        if (itemIconImage != null)
            itemIconImage.color = new Color(1, 1, 1, 0);

        if (amountText != null)
            amountText.gameObject.SetActive(false);
    }
}