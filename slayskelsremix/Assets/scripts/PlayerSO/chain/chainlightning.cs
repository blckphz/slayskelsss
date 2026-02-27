using UnityEngine;

[CreateAssetMenu(fileName = "ChainLightning", menuName = "Abilities/Chain Lightning")]
public class ChainLightning : offensiveRanged
{
    [Header("Chain Settings")]
    public int maxBounces = 3;
    public float bounceRadius = 5f;

    [Header("Stun Settings")]
    public float stunDuration = 2f;
    public float stuneffect = 2f;

    [Header("Tick Damage Settings")]
    public bool isapplyingtickdmg = true;
    public float stundmg = 5f;
    public float stuntick = 0.5f;

    [Tooltip("Degrees to rotate sprite so it visually faces movement")]
    public float rotationOffset = 0f;

    public override void Execute(Transform caster, Transform targetAnchor)
    {
        Vector2 dir = (targetAnchor.position - caster.position).normalized;

        GameObject bolt = ObjectPooler.Instance.GetPooledObject(prefab, caster.position, Quaternion.identity);

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
                isapplyingtickdmg,
                stundmg,
                stuntick
            );
        }
    }
}