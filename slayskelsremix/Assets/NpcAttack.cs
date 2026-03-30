using UnityEngine;

public class NpcAttack : MonoBehaviour
{
    [Header("Ability Configuration")]
    public Ability currentAbility;
    public Transform attackAnchor; // Where the attack originates

    private float nextAttackTime;

    // Use a Trigger to find the enemy instead of manual range math!
    private void OnTriggerStay(Collider other)
    {
        if (currentAbility == null) return;

        // Check if the object we collided with is an Enemy or Resource
        if (other.CompareTag("Enemy") || other.CompareTag("Resource"))
        {
            TryAttack(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Resource"))
        {
            StopAttacking();
        }
    }

    public void TryAttack(Transform target)
    {
        if (Time.time < nextAttackTime) return;

        // Execute attack/combo step
        bool attackFinished = currentAbility.Execute(transform, target, true);

        // FIX: Force a failsafe cooldown so the NPC doesn't get jammed!
        if (attackFinished)
        {
            nextAttackTime = Time.time + currentAbility.fireRate;
        }
        else
        {
            // Failsafe: If it's a combo, we still want a tiny gap so it doesn't spam frame-by-frame
            nextAttackTime = Time.time + 0.1f;
        }
    }

    public void StopAttacking()
    {
        if (currentAbility != null)
        {
            currentAbility.Execute(transform, null, false);
        }
    }
}