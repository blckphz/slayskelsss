using UnityEngine;

public class waterBehav : MonoBehaviour
{
    [SerializeField] private float fillRate = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            Debug.Log("[waterBehav] Not player.");
            return;
        }

        var invManager = InventoryManager.Instance;
        var hotbar = PlayerHotbarManager.Instance;

        if (invManager == null || hotbar == null)
        {
            Debug.LogWarning("[waterBehav] Missing managers.");
            return;
        }

        int index = hotbar.selectedIndex;

        if (index < 0 || index >= invManager.hotbarData.Count)
            return;

        HotbarSlotData slot = invManager.hotbarData[index];

        if (slot == null || slot.item == null)
        {
            Debug.Log("[waterBehav] Empty slot.");
            return;
        }

        Debug.Log($"[waterBehav] Using item: {slot.item.name}");

        // OLD SYSTEM LOGIC (SAFE)

        if (slot.item is buckeSO bucket)
        {
            slot.currentDurability = Mathf.RoundToInt(
     Mathf.Min(
         bucket.maxWater,
         slot.currentDurability + fillRate
     )
 );

            Debug.Log($"[waterBehav] Bucket filled → {slot.currentDurability}");

            hotbar.RefreshHotbar();
            invManager.SaveInventory();

            return;
        }

        Debug.Log("[waterBehav] Not a bucket.");
    }
}