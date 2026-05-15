using UnityEngine;
using System.Collections.Generic;

public class npcJobBrain : MonoBehaviour
{
    private NPCBrain mainBrain;
    private NpcInvBrain inventory;

    void Awake()
    {
        mainBrain = GetComponent<NPCBrain>();
        inventory = GetComponent<NpcInvBrain>();
    }

    public void DetermineNextJob(out NPCBrain.NPCState newState, out Transform target, out string description)
    {
        // Default values
        newState = NPCBrain.NPCState.Wandering;
        target = null;
        description = "Wandering";

        // Find fires that need attention
        CampfireBehav criticalFire = GetFireBelow(0.2f);
        CampfireBehav maintenanceFire = GetFireBelow(0.8f);

        // Determine what fuel we care about right now based on the fire that needs it
        CampfireBehav fireToHelp = criticalFire ?? maintenanceFire;
        ItemData requiredFuel = fireToHelp != null ? fireToHelp.defaultFuelItem : null;

        int fuelInInv = requiredFuel != null ? GetItemCount(requiredFuel.itemID) : 0;

        // 1. SCAVENGING (Check ground for the specific fuel required)
        if (requiredFuel != null)
        {
            PickupItem nearbyItem = GetClosestSpecificPickup(requiredFuel.itemID);
            if (nearbyItem != null && fuelInInv < 20)
            {
                newState = NPCBrain.NPCState.Gathering;
                target = nearbyItem.transform;
                description = $"Scavenging {requiredFuel.itemName}";
                return;
            }
        }

        // 2. EMERGENCY REFUEL
        if (criticalFire != null && requiredFuel != null)
        {
            if (inventory.HasItem(requiredFuel.itemID))
            {
                newState = NPCBrain.NPCState.Refueling;
                target = criticalFire.transform;
                description = $"EMERGENCY: Refueling {requiredFuel.itemName}";
                return;
            }
            else
            {
                Transform chest = GetClosestChestWithItem(requiredFuel.itemID);
                if (chest != null)
                {
                    newState = NPCBrain.NPCState.FetchingFromChest;
                    target = chest;
                    description = $"EMERGENCY: Fetching {requiredFuel.itemName}";
                    return;
                }
            }
        }

        // 3. MAINTENANCE FETCH
        if (requiredFuel != null && fuelInInv == 0 && FireNeedsFuel(mainBrain.maintenanceThreshold))
        {
            Transform chest = GetClosestChestWithItem(requiredFuel.itemID);
            if (chest != null)
            {
                newState = NPCBrain.NPCState.FetchingFromChest;
                target = chest;
                description = $"Fetching {requiredFuel.itemName} supply";
                return;
            }
        }

        // 4. REFUELING (Maintenance)
        if (maintenanceFire != null && requiredFuel != null && inventory.HasItem(requiredFuel.itemID))
        {
            newState = NPCBrain.NPCState.Refueling;
            target = maintenanceFire.transform;
            description = "Maintenance: Refuel";
            return;
        }

        // 5. STOWING
        // Check if we have items that aren't the current required fuel
        int currentRequiredID = requiredFuel != null ? requiredFuel.itemID : -1;
        if (HasExtraItems(currentRequiredID) || (fuelInInv > 0 && maintenanceFire == null))
        {
            ChestInventory chest = GetClosest<ChestInventory>();
            if (chest != null)
            {
                newState = NPCBrain.NPCState.Stowing;
                target = chest.transform;
                description = "Stowing surplus items";
                return;
            }
        }

        // 6. GATHERING (If low on resources generally)
        if (fuelInInv < 15)
        {
            ItemHealth resource = GetClosest<ItemHealth>();
            if (resource != null)
            {
                newState = NPCBrain.NPCState.Gathering;
                target = resource.transform;
                description = "Gathering Resources";
                return;
            }
        }
    }

    // --- Helpers ---
    private int GetItemCount(int itemID)
    {
        var slot = inventory.inventory.Find(s => s.item != null && s.item.itemID == itemID);
        return slot != null ? slot.count : 0;
    }

    private bool HasExtraItems(int currentFuelID)
    {
        // NPC has "extra" if inventory contains anything that isn't the item currently needed for refueling
        return inventory.inventory.Exists(s => s.item != null && s.item.itemID != currentFuelID);
    }

    private bool FireNeedsFuel(float pct)
    {
        CampfireBehav[] fires = Object.FindObjectsByType<CampfireBehav>(FindObjectsSortMode.None);
        foreach (var f in fires)
            if (f.fuelAmount < (f.maxFuel * pct)) return true;
        return false;
    }

    private CampfireBehav GetFireBelow(float pct)
    {
        CampfireBehav[] fires = Object.FindObjectsByType<CampfireBehav>(FindObjectsSortMode.None);
        CampfireBehav worst = null;
        float low = float.MaxValue;
        foreach (var f in fires)
        {
            if (f.fuelAmount < (f.maxFuel * pct) && f.fuelAmount < low)
            {
                low = f.fuelAmount;
                worst = f;
            }
        }
        return worst;
    }

    private Transform GetClosestChestWithItem(int itemID)
    {
        ChestInventory[] chests = Object.FindObjectsByType<ChestInventory>(FindObjectsSortMode.None);
        float d = Mathf.Infinity;
        Transform best = null;
        foreach (var c in chests)
            if (c.HasItem(itemID))
            {
                float dist = Vector2.Distance(transform.position, c.transform.position);
                if (dist < d) { d = dist; best = c.transform; }
            }
        return best;
    }

    private PickupItem GetClosestSpecificPickup(int itemID)
    {
        PickupItem[] pickups = Object.FindObjectsByType<PickupItem>(FindObjectsSortMode.None);
        float d = Mathf.Infinity;
        PickupItem best = null;
        foreach (var p in pickups)
            if (p.GetItemData()?.itemID == itemID)
            {
                float dist = Vector2.Distance(transform.position, p.transform.position);
                if (dist < d && dist < 15f) { d = dist; best = p; }
            }
        return best;
    }

    private T GetClosest<T>() where T : MonoBehaviour
    {
        T[] targets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        float d = Mathf.Infinity;
        T best = null;
        foreach (var t in targets)
        {
            float dist = Vector2.Distance(transform.position, t.transform.position);
            if (dist < d) { d = dist; best = t; }
        }
        return best;
    }
}