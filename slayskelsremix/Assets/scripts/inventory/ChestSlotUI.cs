using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChestSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;

    private ItemData currentItem;
    private int currentCount;

    public void SetSlot(ItemData item, int count)
    {
        currentItem = item;
        currentCount = count;

        if (item == null || count <= 0)
        {
            ClearSlot();
            return;
        }

        icon.sprite = item.icon;
        icon.enabled = true;
        amountText.text = count > 1 ? count.ToString() : "";
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentCount = 0;

        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log($"[ChestSlotUI] Transfer to player: {currentItem.itemName}");

            ChestInventory chest = ChestUI.Instance.GetCurrentChest();

            if (chest == null) return;

            bool removed = chest.RemoveItem(currentItem, 1);

            if (!removed)
            {
                Debug.LogError("[ChestSlotUI] Failed to remove from chest");
                return;
            }

            InvUI.Instance.inventoryManager.AddItem(currentItem, 1);

            ChestUI.Instance.Refresh();
            InvUI.Instance.RefreshUI();
        }
    }
}