using UnityEngine;

public class ResourceBehav : ItemHealth
{
    // This script inherits all the hit-effects from ItemHealth.
    // To make it behave like wood, we ensure Die() triggers the correct loot pop.

    protected override void Die()
    {

        base.Die();
    }

    // SpawnLoot is already handled by ItemHealth. 
    // Simply assign your 'Stone_Prefab' to the 'Loot Prefab' slot in the Inspector.
}