using UnityEngine;

public class npcHealth : healthMaster
{

    protected override void Die()
    {
        // Find player LevelManager
        LevelManager player = FindFirstObjectByType<LevelManager>();

        if (player != null)
        {
            player.AddXP(xpReward);
        }

        // destroy NPC
        base.Die();
    }
}