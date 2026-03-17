using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAbility", menuName = "Abilities/Melee")]
public class offensivemelee : offensiveability
{
    [Header("Melee Stats")]
    public int maxSwings = 3;
    public float swingFreq = 0.2f; // Speed of swings during hold
    public float spawnOffset = 1.5f;
    public float rotationOffset = 0f;

    private int currentSwingCount = 0;
    private float nextSwingReadyTime = 0f;
    private int toggleIndex = 0;

    public virtual float GetBonusDamage() => 0f;

    public override void Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // 1. Reset combo if not holding
        if (!isHolding)
        {
            currentSwingCount = 0;
            // Reset internal ready time so the next first click is instant
            if (Time.time > nextSwingReadyTime) nextSwingReadyTime = 0;
            return;
        }

        // 2. Combo Logic
        if (Time.time >= nextSwingReadyTime && currentSwingCount < maxSwings)
        {
            PerformSwing(caster, targetAnchor);

            currentSwingCount++;
            nextSwingReadyTime = Time.time + swingFreq;
        }
    }

    private void PerformSwing(Transform caster, Transform targetAnchor)
    {
        if (prefab == null) return;
        toggleIndex++;

        // Get direction
        Vector3 targetPos = targetAnchor != null ? targetAnchor.position : caster.position + caster.right;
        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;

        // Snapping logic for 4-way direction
        Vector2 snappedDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0)
            : new Vector2(0, Mathf.Sign(dir.y));

        Vector3 spawnPos = caster.position + (Vector3)(snappedDir * spawnOffset);
        float angle = (Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg) + rotationOffset;

        // Spawn
        GameObject woosh = ObjectPooler.Instance.GetPooledObject(prefab, spawnPos, Quaternion.Euler(0, 0, angle));
        if (woosh == null) return;

        woosh.transform.SetParent(caster);
        woosh.transform.localPosition = caster.InverseTransformPoint(spawnPos);

        if (launchsound != null) audiomanager.Instance?.PlaySound(launchsound);

        var behav = woosh.GetComponent<meleebehav>();
        if (behav != null) behav.Setup(damage, GetBonusDamage(), toggleIndex);

        CameraShaker.Shake(0.4f, 0.12f);
    }
}