using UnityEngine;

public abstract class offensivemelee : offensiveability, IItemDescriptionProvider
{
    public enum SwingOwner { Player, NPC }

    [Header("Melee Settings")]
    public int maxSwings = 3;
    public float swingFreq = 0.2f;
    public bool customAudioLogic = false;

    [Header("Spawn Settings")]
    public float spawnOffset = 1.5f;
    public float rotationOffset = 0f;


    private int currentSwingIndex;
    private float nextSwingTime;
    private float cooldownEndTime;

    public bool IsOnCooldown => Time.time < cooldownEndTime;

    // =====================================================
    // IItemDescriptionProvider
    // =====================================================

    public virtual string GetDetailedDescription()
    {
        return $"<color=#FF6B6B>Damage:</color> {damage}\n" +
               $"<color=#A2E8DD>Swing Rate:</color> {fireRate}s\n" +
               "------------------\n" +
               "<color=#FFA500><b>[PRIMARY ACTION]</b></color>\n" +
               "• Swing: Melee attack with combo system.";
    }

    public virtual float GetBonusDamage() => 0f;

    // =====================================================
    // INIT
    // =====================================================

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
    }

    // =====================================================
    // EXECUTE
    // =====================================================

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (caster == null || !isHolding) return false;
        if (ActionLock.IsLocked || IsOnCooldown || Time.time < nextSwingTime) return false;
        if (BuildManager.Instance != null && BuildManager.Instance.IsPlacing) return false;

        SwingOwner owner = caster.GetComponent<PlayerAttack>() != null
            ? SwingOwner.Player
            : SwingOwner.NPC;

        if (!customAudioLogic && AudioManager.Instance != null && launchsound != null)
            AudioManager.Instance.PlaySound(launchsound, 1f);

        meleebehav swing = PerformSwing(caster, targetAnchor, currentSwingIndex, owner);


        currentSwingIndex++;
        nextSwingTime = Time.time + swingFreq;

        if (currentSwingIndex >= maxSwings)
        {
            currentSwingIndex = 0;

            float finalCooldown = fireRate;

            cooldownEndTime = Time.time + finalCooldown;

            return true;
        }

        return false;
    }

    // =====================================================
    // SWING CREATION (VERTICAL ONLY FIX)
    // =====================================================

    public meleebehav PerformSwing(Transform caster, Transform targetAnchor, int index, SwingOwner owner)
    {
        if (prefab == null || ObjectPooler.Instance == null) return null;

        Vector3 targetPos = targetAnchor != null
            ? targetAnchor.position
            : caster.position + caster.right;

        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;

        // =====================================================
        // ✅ NORTH / SOUTH ONLY MELEE LOGIC
        // =====================================================
        Vector2 snappedDir;

        if (Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))
        {
            // vertical attack (allowed)
            snappedDir = new Vector2(0, Mathf.Sign(dir.y));
        }
        else
        {
            // horizontal input is forced into vertical (fallback)
            snappedDir = new Vector2(0, dir.y >= 0 ? 1 : -1);
        }

        Vector3 offset = (Vector3)(snappedDir * spawnOffset);
        float angle = Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg + rotationOffset;

        GameObject woosh = ObjectPooler.Instance.GetPooledObject(
            prefab,
            caster.position + offset,
            Quaternion.Euler(0, 0, angle)
        );

        if (woosh == null) return null;

        woosh.SetActive(true);

        int bonus = owner == SwingOwner.Player
            ? Mathf.RoundToInt(GetBonusDamage())
            : 0;

        var behav = woosh.GetComponent<meleebehav>();

        behav.Setup(
            damage,
            bonus,
            index,
            owner,
            caster,
            offset,
            (owner == SwingOwner.Player && this is ToolsSO toolSO)
                ? toolSO.toolType
                : ToolType.None
        );

        if (owner == SwingOwner.Player && CameraShaker.Instance != null)
            CameraShaker.Instance.Shake(0.15f, 0.1f);

        return behav;
    }

    // =====================================================
    // COMBO
    // =====================================================

}