using UnityEngine;

public class ResourceBehav : ItemHealth
{
    // This script inherits all the hit-effects from ItemHealth.
    // To make it behave like wood, we ensure Die() triggers the correct loot pop.

    protected override void Die()
    {
        // 1. Any resource-specific death VFX (like a rock shattering) goes here.

        // 2. Call the base Die which handles:
        //    - Notifying NPCs
        //    - Running SpawnLoot()
        //    - Destroying the world object
        base.Die();
    }

    // SpawnLoot is already handled by ItemHealth. 
    // Simply assign your 'Stone_Prefab' to the 'Loot Prefab' slot in the Inspector.
}