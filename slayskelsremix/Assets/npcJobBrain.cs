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
        newState = NPCBrain.NPCState.Wandering;
        target = null;
        description = "Wandering";

        CampfireBehav criticalFire = GetFireBelow(0.2f);
        CampfireBehav maintenanceFire = GetFireBelow(0.8f);

        CampfireBehav fireToHelp = criticalFire ?? maintenanceFire;

        ItemData requiredFuel =
            fireToHelp != null && fireToHelp.validFuelItems.Count > 0
                ? fireToHelp.validFuelItems[0]
                : null;

        int fuelInInv = requiredFuel != null ? GetItemCount(requiredFuel.itemID) : 0;

        if (requiredFuel != null)
        {
            PickupItem nearbyItem = GetClosestSpecificPickup(requiredFuel.itemID);

            if (nearbyItem != null && fuelInInv < 20)
            {
                newState = NPCBrain.NPCState.Gathering;
                target = nearbyItem.transform;
                description = $"Scavenging fuel";
                return;
            }
        }

        if (criticalFire != null && inventory.HasItem(requiredFuel.itemID))
        {
            newState = NPCBrain.NPCState.Refueling;
            target = criticalFire.transform;
            description = "Emergency Refuel";
            return;
        }

        if (maintenanceFire != null && inventory.HasItem(requiredFuel.itemID))
        {
            newState = NPCBrain.NPCState.Refueling;
            target = maintenanceFire.transform;
            description = "Maintenance Refuel";
            return;
        }
    }

    private int GetItemCount(int itemID)
    {
        var slot = inventory.inventory.Find(s => s.item != null && s.item.itemID == itemID);
        return slot != null ? slot.count : 0;
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

    private PickupItem GetClosestSpecificPickup(int itemID)
    {
        PickupItem[] pickups = Object.FindObjectsByType<PickupItem>(FindObjectsSortMode.None);

        float d = Mathf.Infinity;
        PickupItem best = null;

        foreach (var p in pickups)
        {
            if (p.GetItemData()?.itemID == itemID)
            {
                float dist = Vector2.Distance(transform.position, p.transform.position);
                if (dist < d && dist < 15f)
                {
                    d = dist;
                    best = p;
                }
            }
        }

        return best;
    }
}