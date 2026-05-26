using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAbility", menuName = "Abilities/Melee")]
public class offensivemelee : offensiveability
{
    public enum SwingOwner { Player, NPC }

    public bool customAudioLogic = false;

    [Header("Melee Stats")]
    public int maxSwings = 3;
    public float swingFreq = 0.2f;

    [Header("Spawn")]
    public float spawnOffset = 1.5f;
    public float rotationOffset = 0f;

    private int currentSwingIndex;
    private float nextSwingTime;
    private float cooldownEndTime;

    public virtual float GetBonusDamage() => 0f;

    private void OnEnable()
    {
        ResetMeleeState();
    }

    public void ResetMeleeState()
    {
        currentSwingIndex = 0;
        nextSwingTime = 0f;
        cooldownEndTime = 0f;
        isOnCooldown = false;
    }

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (caster == null || !isHolding)
            return false;

        // ❌ Global locks
        if (BuildManager.Instance != null && BuildManager.Instance.IsPlacing)
            return false;

        if (ActionLock.IsLocked)
            return false;

        // 🔥 HARD SAFETY RESET (prevents “stuck melee forever” bug)
        if (currentSwingIndex < 0 || currentSwingIndex > maxSwings)
            ResetMeleeState();

        // 🔥 cooldown check
        if (isOnCooldown)
        {
            if (Time.time < cooldownEndTime)
                return false;

            isOnCooldown = false;
        }

        // 🔥 swing rate limit
        if (Time.time < nextSwingTime)
            return false;

        SwingOwner owner =
            caster.GetComponent<PlayerAttack>() != null
                ? SwingOwner.Player
                : SwingOwner.NPC;

        PerformSwing(caster, targetAnchor, currentSwingIndex, owner);

        currentSwingIndex++;
        nextSwingTime = Time.time + swingFreq;

        // combo finished → cooldown
        if (currentSwingIndex >= maxSwings)
        {
            currentSwingIndex = 0;
            isOnCooldown = true;
            cooldownEndTime = Time.time + fireRate;

            return true;
        }

        return false;
    }

    public void PerformSwing(
        Transform caster,
        Transform targetAnchor,
        int index,
        SwingOwner owner)
    {
        if (prefab == null || ObjectPooler.Instance == null)
            return;

        Vector3 targetPos =
            targetAnchor != null
                ? targetAnchor.position
                : caster.position + caster.right;

        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;

        Vector2 snappedDir =
            Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                ? new Vector2(Mathf.Sign(dir.x), 0)
                : new Vector2(0, Mathf.Sign(dir.y));

        Vector3 offset = (Vector3)(snappedDir * spawnOffset);
        Vector3 spawnPos = caster.position + offset;

        float angle =
            Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg
            + rotationOffset;

        GameObject woosh = ObjectPooler.Instance.GetPooledObject(
            prefab,
            spawnPos,
            Quaternion.Euler(0, 0, angle)
        );

        if (woosh == null) return;

        woosh.SetActive(true);

        float bonus = owner == SwingOwner.Player ? GetBonusDamage() : 0f;

        ToolType tool = ToolType.Axe;
        if (this is ToolsSO toolSO)
            tool = toolSO.toolType;

        var behav = woosh.GetComponent<meleebehav>();
        if (behav != null)
        {
            behav.Setup(
                damage,
                bonus,
                index,
                owner,
                caster,
                offset,
                tool
            );
        }

        if (owner == SwingOwner.Player && CameraShaker.Instance != null)
            CameraShaker.Instance.Shake(0.15f, 0.1f);
    }
}