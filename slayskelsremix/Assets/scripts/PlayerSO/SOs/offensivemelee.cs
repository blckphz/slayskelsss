using UnityEngine;

public abstract class offensivemelee : offensiveability, IItemDescriptionProvider
{
    public enum SwingOwner { Player, NPC }

    [Header("Melee Stats")]
    public int maxSwings = 3;
    public float swingFreq = 0.2f;
    public bool customAudioLogic = false;

    [Header("Spawn")]
    public float spawnOffset = 1.5f;
    public float rotationOffset = 0f;

    [Header("Miss Recovery")]
    [Range(0f, 1f)]
    public float missCooldownMultiplier = 0.65f;

    private int currentSwingIndex;
    private float nextSwingTime;
    private float cooldownEndTime;
    private bool comboConnected;

    public bool IsOnCooldown => Time.time < cooldownEndTime;

    // --- Abstraction for Tool/Weapon Identification ---
    public virtual ToolType GetToolType() => ToolType.None;

    // --- IItemDescriptionProvider Implementation ---
    public virtual string GetDetailedDescription()
    {
        return $"<color=#FF6B6B>Damage:</color> {damage}\n" +
               $"<color=#A2E8DD>Swing Rate:</color> {fireRate}s\n" +
               "------------------\n" +
               "<color=#FFA500><b>[PRIMARY ACTION]</b></color>\n" +
               "• Swing: Melee attack with potential combo sequence.";
    }

    public virtual float GetBonusDamage() => 0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetMeleeState();
    }

    public void ResetMeleeState()
    {
        currentSwingIndex = 0;
        nextSwingTime = 0f;
        cooldownEndTime = 0f;
        comboConnected = false;
    }

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (caster == null || !isHolding) return false;
        if (ActionLock.IsLocked || IsOnCooldown || Time.time < nextSwingTime) return false;
        if (BuildManager.Instance != null && BuildManager.Instance.IsPlacing) return false;

        SwingOwner owner = caster.GetComponent<PlayerAttack>() != null ? SwingOwner.Player : SwingOwner.NPC;

        // Audio
        if (!customAudioLogic && AudioManager.Instance != null && launchsound != null)
            AudioManager.Instance.PlaySound(launchsound, 1f);

        // Perform Swing
        meleebehav swing = PerformSwing(caster, targetAnchor, currentSwingIndex, owner);

        if (swing != null)
            swing.OnSwingFinished = OnSwingFinished;

        currentSwingIndex++;
        nextSwingTime = Time.time + swingFreq;

        // Finish Combo
        if (currentSwingIndex >= maxSwings)
        {
            currentSwingIndex = 0;
            float finalCooldown = comboConnected ? fireRate : fireRate * missCooldownMultiplier;
            cooldownEndTime = Time.time + finalCooldown;
            comboConnected = false;
            return true;
        }

        return false;
    }

    private void OnSwingFinished(bool hit)
    {
        comboConnected |= hit;
    }

    public meleebehav PerformSwing(Transform caster, Transform targetAnchor, int index, SwingOwner owner)
    {
        if (prefab == null || ObjectPooler.Instance == null) return null;

        Vector3 targetPos = targetAnchor != null ? targetAnchor.position : caster.position + caster.right;
        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;
        Vector2 snappedDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0)
            : new Vector2(0, Mathf.Sign(dir.y));

        Vector3 offset = (Vector3)(snappedDir * spawnOffset);
        float angle = Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg + rotationOffset;

        GameObject woosh = ObjectPooler.Instance.GetPooledObject(prefab, caster.position + offset, Quaternion.Euler(0, 0, angle));

        if (woosh == null) return null;

        woosh.SetActive(true);

        int bonus = owner == SwingOwner.Player ? Mathf.RoundToInt(GetBonusDamage()) : 0;
        var behav = woosh.GetComponent<meleebehav>();

        if (behav != null)
        {
            behav.Setup(damage, bonus, index, owner, caster, offset, GetToolType());
        }

        if (owner == SwingOwner.Player && CameraShaker.Instance != null)
            CameraShaker.Instance.Shake(0.15f, 0.1f);

        return behav;
    }
}