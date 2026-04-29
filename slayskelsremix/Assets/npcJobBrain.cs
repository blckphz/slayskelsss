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

        int woodCount = GetWoodCount();

        // 1. SCAVENGING
        PickupItem nearbyItem = GetClosestWoodPickup();
        if (nearbyItem != null && woodCount < 20)
        {
            newState = NPCBrain.NPCState.Gathering;
            target = nearbyItem.transform;
            description = "Scavenging ground items";
            Debug.Log($"<color=yellow>[Decision]</color> Found wood on ground. Priority: Scavenge.");
            return;
        }

        // 2. EMERGENCY REFUEL
        CampfireBehav criticalFire = GetFireBelow(0.2f);
        if (criticalFire != null)
        {
            if (inventory.HasItem(mainBrain.woodItemID))
            {
                newState = NPCBrain.NPCState.Refueling;
                target = criticalFire.transform;
                description = "EMERGENCY: Refueling fire";
                Debug.Log("<color=red>[Decision]</color> Fire is dying! Priority: Emergency Refuel.");
                return;
            }
            else
            {
                Transform chest = GetClosestChestWithWood();
                if (chest != null)
                {
                    newState = NPCBrain.NPCState.FetchingFromChest;
                    target = chest;
                    description = "EMERGENCY: Fetching wood";
                    Debug.Log("<color=red>[Decision]</color> Fire dying but I have no wood. Priority: Emergency Fetch.");
                    return;
                }
            }
        }

        // 3. MAINTENANCE FETCH
        if (woodCount == 0 && FireNeedsFuel(mainBrain.maintenanceThreshold))
        {
            Transform chest = GetClosestChestWithWood();
            if (chest != null)
            {
                newState = NPCBrain.NPCState.FetchingFromChest;
                target = chest;
                description = "Fetching wood supply";
                Debug.Log("<color=orange>[Decision]</color> Fires need fuel soon. Priority: Maintenance Fetch.");
                return;
            }
        }

        // 4. REFUELING
        CampfireBehav maintenanceFire = GetFireBelow(0.8f);
        if (inventory.HasItem(mainBrain.woodItemID) && maintenanceFire != null)
        {
            newState = NPCBrain.NPCState.Refueling;
            target = maintenanceFire.transform;
            description = "Maintenance: Refuel";
            return;
        }

        // 5. STOWING
        if (HasExtraItems() || (woodCount > 0 && maintenanceFire == null))
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

        // 6. GATHERING
        if (woodCount < 15)
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

        // 7. FALLBACK
        Debug.Log("<color=white>[Decision]</color> No tasks found. Priority: Wandering.");
    }

    // --- Helpers ---
    private int GetWoodCount()
    {
        var slot = inventory.inventory.Find(s => s.item != null && s.item.itemID == mainBrain.woodItemID);
        return slot != null ? slot.count : 0;
    }

    private bool HasExtraItems() => inventory.inventory.Exists(s => s.item != null && s.item.itemID != mainBrain.woodItemID);

    private bool FireNeedsFuel(float pct)
    {
        CampfireBehav[] fires = Object.FindObjectsByType<CampfireBehav>(FindObjectsSortMode.None);
        foreach (var f in fires) if (f.fuelAmount < (f.maxFuel * pct)) return true;
        return false;
    }

    private CampfireBehav GetFireBelow(float pct)
    {
        CampfireBehav[] fires = Object.FindObjectsByType<CampfireBehav>(FindObjectsSortMode.None);
        CampfireBehav worst = null; float low = float.MaxValue;
        foreach (var f in fires) if (f.fuelAmount < (f.maxFuel * pct) && f.fuelAmount < low) { low = f.fuelAmount; worst = f; }
        return worst;
    }

    private Transform GetClosestChestWithWood()
    {
        ChestInventory[] chests = Object.FindObjectsByType<ChestInventory>(FindObjectsSortMode.None);
        float d = Mathf.Infinity; Transform best = null;
        foreach (var c in chests) if (c.HasItem(mainBrain.woodItemID))
            {
                float dist = Vector2.Distance(transform.position, c.transform.position);
                if (dist < d) { d = dist; best = c.transform; }
            }
        return best;
    }

    private PickupItem GetClosestWoodPickup()
    {
        PickupItem[] pickups = Object.FindObjectsByType<PickupItem>(FindObjectsSortMode.None);
        float d = Mathf.Infinity; PickupItem best = null;
        foreach (var p in pickups) if (p.GetItemData()?.itemID == mainBrain.woodItemID)
            {
                float dist = Vector2.Distance(transform.position, p.transform.position);
                if (dist < d && dist < 15f) { d = dist; best = p; }
            }
        return best;
    }

    private T GetClosest<T>() where T : MonoBehaviour
    {
        T[] targets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        float d = Mathf.Infinity; T best = null;
        foreach (var t in targets)
        {
            float dist = Vector2.Distance(transform.position, t.transform.position);
            if (dist < d) { d = dist; best = t; }
        }
        return best;
    }
}