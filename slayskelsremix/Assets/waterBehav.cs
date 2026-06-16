using UnityEngine;

public class waterBehav : MonoBehaviour
{
    [SerializeField] private float fillRate = 10f;
    [SerializeField] private AudioClip fillSound;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var invManager = InventoryManager.Instance;
        var hotbar = PlayerHotbarManager.Instance;

        if (invManager == null || hotbar == null)
            return;

        int index = hotbar.selectedIndex;

        if (index < 0 || index >= invManager.hotbarData.Count)
            return;

        HotbarSlotData slot = invManager.hotbarData[index];

        if (slot == null || slot.item == null)
            return;

        if (slot.item is buckeSO bucket)
        {
            int oldValue = slot.currentDurability;

            float newValue = Mathf.Min(
                bucket.maxWater,
                slot.currentDurability + fillRate
            );

            slot.currentDurability = Mathf.RoundToInt(newValue);

            bool wasFull = oldValue >= bucket.maxWater;
            bool isFullNow = slot.currentDurability >= bucket.maxWater;

            Debug.Log($"[waterBehav] Bucket filled → {slot.currentDurability}");

            // ✅ play ONLY when it *just became full*
            if (!wasFull && isFullNow)
            {
                AudioManager.Instance?.PlaySound(fillSound);
            }

            hotbar.RefreshHotbar();
            invManager.SaveInventory();

            return;
        }
    }
}