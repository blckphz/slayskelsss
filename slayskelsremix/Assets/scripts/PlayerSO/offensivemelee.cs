using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAbility", menuName = "Abilities/Melee")]
public class offensivemelee : offensiveability
{
    [Header("Melee Stats")]
    public int maxSwings = 3;
    public float swingFreq = 0.2f;
    public float spawnOffset = 1.5f;
    public float rotationOffset = 0f;

    private int currentSwingCount = 0;
    private float nextSwingReadyTime = 0f;
    private int toggleIndex = 0;

    public virtual float GetBonusDamage() => 0f;

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // 1. Reset combo if not holding
        if (!isHolding)
        {
            currentSwingCount = 0;
            if (Time.time > nextSwingReadyTime) nextSwingReadyTime = 0;
            return false;
        }

        // 2. Combo Logic
        if (Time.time >= nextSwingReadyTime && currentSwingCount < maxSwings)
        {
            PerformSwing(caster, targetAnchor);
            currentSwingCount++;
            nextSwingReadyTime = Time.time + swingFreq;

            // 3. AUTO-COOLDOWN SIGNAL: If we reached the max, reset and tell the controller to start CD
            if (currentSwingCount >= maxSwings)
            {
                currentSwingCount = 0;
                return true;
            }
        }
        return false;
    }

    private void PerformSwing(Transform caster, Transform targetAnchor)
    {
        if (prefab == null) return;
        toggleIndex++;

        Vector3 targetPos = targetAnchor != null ? targetAnchor.position : caster.position + caster.right;
        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;

        Vector2 snappedDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0)
            : new Vector2(0, Mathf.Sign(dir.y));

        Vector3 spawnPos = caster.position + (Vector3)(snappedDir * spawnOffset);
        float angle = (Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg) + rotationOffset;

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