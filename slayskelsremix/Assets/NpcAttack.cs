using UnityEngine;

public class NpcAttack : MonoBehaviour
{
    [Header("Ability Configuration")]
    public Ability currentAbility;
    public Transform attackAnchor; // A child object or offset where the attack originates

    private float nextAttackTime;
    private bool isExecutingCombo = false;

    public void TryAttack(Transform target)
    {
        if (currentAbility == null || target == null) return;

        // Check if global cooldown/fireRate has passed
        if (Time.time >= nextAttackTime)
        {
            // Execute returns true when a full combo/attack cycle is finished
            // We pass 'true' for isHolding to simulate the NPC "pressing the button"
            bool attackFinished = currentAbility.Execute(transform, target, true);

            if (attackFinished)
            {
                // Set the cooldown based on the ability's fireRate
                nextAttackTime = Time.time + currentAbility.fireRate;
            }
        }
    }

    // Helper to stop attacking (resets combos)
    public void StopAttacking()
    {
        if (currentAbility != null)
        {
            currentAbility.Execute(transform, null, false);
        }
    }
}