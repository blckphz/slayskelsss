using UnityEngine;
using static UnityEngine.Rendering.SplashScreen;

[CreateAssetMenu(fileName = "ThrownStone", menuName = "Abilities/Thrown Stone")]
public class StoneSO : offensiveRanged
{
    [Header("Stone Specifics")]
    public float knockbackForce = 5f;

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (prefab == null) return false;

        // Calculate direction toward the target
        Vector2 direction = (targetAnchor.position - caster.position).normalized;

        // Spawn from pool
        GameObject stone = ObjectPooler.Instance.GetPooledObject(prefab, caster.position, Quaternion.identity);

        // Get the behavior component to pass data
        var behavior = stone.GetComponent<StoneBehavior>();
        if (behavior != null)
        {
            behavior.Setup(
                damage,
                projectileSpeed,
                direction,
                pierceCount,
                knockbackForce
            );
        }

        return true;
    }
}