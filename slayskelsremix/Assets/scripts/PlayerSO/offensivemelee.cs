using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAbility", menuName = "Abilities/Melee")]
public class offensivemelee : offensiveability
{
    // 🔥 Enum INSIDE the class (as requested)
    public enum SwingOwner
    {
        Player,
        NPC
    }

    [Tooltip("Check this if the ability handles its own sound timing (like melee combos).")]
    public bool customAudioLogic = false;

    [Header("Melee Stats")]
    public int maxSwings = 3;
    public float swingFreq = 0.2f;

    [Header("Spawn")]
    public float spawnOffset = 1.5f;
    public float rotationOffset = 0f;

    public virtual float GetBonusDamage() => 0f;

    // runtime state (per instance)
    private int swingIndex;
    private float nextSwingTime;
    private bool isActive;

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (caster == null)
            return false;

        SwingOwner owner =
            caster.GetComponent<PlayerAttack>() != null
            ? SwingOwner.Player
            : SwingOwner.NPC;


        if (isHolding && !isActive)
        {
            isActive = true;
            swingIndex = 0;
            nextSwingTime = Time.time;

        }

        if (!isHolding)
        {
            isActive = false;
            return false;
        }

        if (!isActive || Time.time < nextSwingTime)
            return false;

        PerformSwing(caster, targetAnchor, swingIndex, owner);

        swingIndex++;
        nextSwingTime = Time.time + swingFreq;


        if (swingIndex >= maxSwings)
        {
            isActive = false;
            return true;
        }

        return false;
    }

    private void PerformSwing(Transform caster, Transform targetAnchor, int index, SwingOwner owner)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[Melee] Missing prefab");
            return;
        }

        Vector3 targetPos = targetAnchor != null
            ? targetAnchor.position
            : caster.position + caster.right;

        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;

        Vector2 snappedDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0)
            : new Vector2(0, Mathf.Sign(dir.y));

        Vector3 spawnPos = caster.position + (Vector3)(snappedDir * spawnOffset);
        float angle = Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg + rotationOffset;

        GameObject woosh = ObjectPooler.Instance.GetPooledObject(
            prefab,
            spawnPos,
            Quaternion.Euler(0, 0, angle)
        );

        if (woosh == null)
        {
            Debug.LogWarning("[Melee] Pool returned NULL");
            return;
        }

        woosh.transform.SetParent(caster);
        woosh.transform.localPosition = caster.InverseTransformPoint(spawnPos);

        float bonus = 0f;

        if (owner == SwingOwner.Player)
        {
            bonus = GetBonusDamage();
        }

        var behav = woosh.GetComponent<meleebehav>();
        if (behav != null)
        {
            behav.Setup(damage, bonus, index, owner);
        }



        CameraShaker.Shake(0.4f, 0.12f);
    }

    public string GetStatsFormat()
    {
        return $"Combo Swings: {maxSwings}\n" +
               $"Swing Speed: {swingFreq}s";
    }

}