using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CampfireSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public CampfireBehav campfire;

    [Header("UI")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Pulse")]
    [SerializeField] private invUIBgPulse fuelPulse;

    private void Start()
    {
        campfire = FindAnyObjectByType<CampfireBehav>();
    }

    private void OnEnable()
    {
        if (campfire != null)
            campfire.OnPulseStateChanged += HandlePulse;

        HandlePulse(campfire != null && campfire.HasFuel);
    }

    private void OnDisable()
    {
        if (campfire != null)
            campfire.OnPulseStateChanged -= HandlePulse;
    }

    private void HandlePulse(bool shouldPulse)
    {
        if (fuelPulse != null)
            fuelPulse.SetPulsing(shouldPulse);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (campfire == null) return;
        if (campfire.fuelAmount < 1f) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int removed = campfire.RemoveFuel(1);

            if (removed > 0)
            {
                InventoryManager.Instance.AddItem(campfire.currentFuelItem, removed);

                PlayerHotbarManager.Instance?.SyncHotbarToData();
                InventoryManager.Instance.SaveInventory();
                BuildingSaveManager.Instance?.SaveNow();

                UpdateUI();
                InvUI.Instance?.RefreshUI();
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (campfire == null) return;

        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot == null) return;

        ItemData item = draggedSlot.GetItem();
        if (item == null) return;

        if (!campfire.IsValidFuel(item))
            return;

        int added = campfire.AddFuel(item, draggedSlot.GetCount());

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

    public void UpdateUI()
    {
        if (campfire == null) return;

        if (campfire.fuelAmount <= 0 || campfire.currentFuelItem == null)
        {
            ResetSlot();
            return;
        }

        if (itemIconImage != null)
        {
            itemIconImage.sprite = campfire.currentFuelItem.icon;
            itemIconImage.enabled = true;

            Color c = itemIconImage.color;
            c.a = 1f;
            itemIconImage.color = c;
        }

        if (amountText != null)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = Mathf.CeilToInt(campfire.fuelAmount).ToString();
        }
    }

    public void ResetSlot()
    {
        if (itemIconImage != null)
        {
            itemIconImage.sprite = emptySprite;

            Color c = itemIconImage.color;
            c.a = 0f;
            itemIconImage.color = c;
        }

        if (amountText != null)
            amountText.gameObject.SetActive(false);
    }
}