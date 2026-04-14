using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CampfireSlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler
{
    [Header("References")]
    public CampfireBehav campfire;
    public int woodID;

    [Header("UI")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private TextMeshProUGUI amountText;

    private void Start()
    {
        UpdateUI();
    }

    // ---------------------------------------------------
    // DROP WHOLE STACK
    // ---------------------------------------------------
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        InventorySlotUI draggedSlot =
            eventData.pointerDrag.GetComponentInParent<InventorySlotUI>();

        if (draggedSlot == null)
            return;

        ItemData item = draggedSlot.GetItem();
        if (item == null || item.itemID != woodID)
            return;

        if (campfire == null)
            return;

        int stackAmount = draggedSlot.GetCount();

        int added = campfire.AddFuel(item, stackAmount);

        if (added <= 0)
        {
            Debug.Log("Campfire full or wrong item");
            return;
        }

        InventoryManager.Instance.RemoveItem(item, added);

        UpdateUI();

        if (!campfire.isBurning)
            campfire.Ignite();

        InvUI.Instance?.RefreshUI();
    }

    // ---------------------------------------------------
    // UI UPDATE
    // ---------------------------------------------------
    public void UpdateUI()
    {
        if (campfire == null || campfire.fuelItem == null || campfire.fuelAmount <= 0)
        {
            ResetSlot();
            return;
        }

        if (itemIconImage != null)
        {
            itemIconImage.enabled = true;
            itemIconImage.sprite = campfire.fuelItem.icon;
            itemIconImage.color = Color.white;
        }

        if (amountText != null)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = campfire.fuelAmount > 1
                ? campfire.fuelAmount.ToString()
                : "";
        }
    }

    // ---------------------------------------------------
    // RESET (EMPTY / OUT OF RANGE FIX)
    // ---------------------------------------------------
    public void ResetSlot()
    {
        if (itemIconImage != null)
        {
            itemIconImage.enabled = true;
            itemIconImage.sprite = emptySprite;
            itemIconImage.color = emptySprite != null
                ? Color.white
                : new Color(1f, 1f, 1f, 0f);
        }

        if (amountText != null)
        {
            amountText.text = "";
            amountText.gameObject.SetActive(false); // 🔥 IMPORTANT FIX
        }
    }

    // ---------------------------------------------------
    // FORCE HIDE (OUT OF RANGE CALL)
    // ---------------------------------------------------
    public void HideOutsideRange()
    {
        if (itemIconImage != null)
            itemIconImage.enabled = false;

        if (amountText != null)
        {
            amountText.text = "";
            amountText.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("🟡 Pointer Enter");
    }
}