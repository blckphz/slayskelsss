using UnityEngine;

[CreateAssetMenu(fileName = "ChainLightning", menuName = "Abilities/Chain Lightning")]
public class ChainLightning : offensiveRanged
{
    [Header("Chain Settings")]
    public int maxBounces = 3;
    public float bounceRadius = 5f;

    [Header("Stun Settings")]
    public float stunDuration;
    public float stuneffect;

    public bool isapplyingtickdmg;

    public float stundmg = 5f;
    public float stuntick = 5f;


    [Tooltip("Degrees to rotate sprite so it visually faces movement")]
    public float rotationOffset = 0f;

    public override void Execute(Transform caster, Transform targetAnchor)
    {
        // 1. Calculate direction
        Vector2 dir = (targetAnchor.position - caster.position).normalized;

        // 2. Use the Pooler instead of Instantiate
        // We pass the prefab as the key so the pooler knows which "drawer" to look in
        GameObject bolt = ObjectPooler.Instance.GetPooledObject(prefab, caster.position, Quaternion.identity);

        // 3. Initialize the projectile behavior
        var behav = bolt.GetComponent<chainlightningBehav>();
        if (behav != null)
        {
            behav.Setup(
                damage,
                maxBounces,
                bounceRadius,
                dir * projectileSpeed,
                rotationOffset,
                stunDuration,
                stuneffect,
                isapplyingtickdmg

            );
        }
    }
}