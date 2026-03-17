using UnityEngine;

[CreateAssetMenu(fileName = "New 2D Grenade", menuName = "Abilities/2D Grenade")]
public class grenadeSO : offensiveRanged
{
    public float throwForce = 10f;
    public float explosionRadius = 3f;

    // Added 'bool isHolding' to match the updated Ability base class
    public override void Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // For grenades, we typically only want to throw one per click.
        // The PlayerAttack script handles the fireRate cooldown, 
        // so we don't need to check 'isHolding' here unless you want to add a charging mechanic.

        if (prefab == null)
        {
            Debug.LogError("[GrenadeSO] Prefab is missing on the ScriptableObject!");
            return;
        }

        GameObject grenade = Instantiate(prefab, caster.position, Quaternion.identity);

        // Logic for throw direction
        Vector2 throwDir = Vector2.right; // Default fallback
        if (targetAnchor != null)
        {
            throwDir = (targetAnchor.position - caster.position).normalized;
        }

        // Apply physics
        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(throwDir * throwForce, ForceMode2D.Impulse);
        }

        // Initialize logic and static tracking
        if (grenade.TryGetComponent(out grenadeBehav logic))
        {
            logic.Initialize(damage, explosionRadius, logic.fuseTime);
        }
    }
}