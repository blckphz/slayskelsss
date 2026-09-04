using UnityEngine;

public abstract class offensivemelee : offensiveability, IItemDescriptionProvider
{
    public enum SwingOwner
    {
        Player,
        NPC
    }

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


    // =========================================================
    // DESCRIPTION
    // =========================================================

    public virtual string GetDetailedDescription()
    {
        return $"<color=#FF6B6B>Damage:</color> {damage}\n" +
               $"<color=#A2E8DD>Swing Rate:</color> {fireRate}s\n" +
               "------------------\n" +
               "<color=#FFA500><b>[PRIMARY ACTION]</b></color>\n" +
               "• Swing: Melee attack with combo system.";
    }

    public virtual float GetBonusDamage()
    {
        return 0f;
    }


    // =========================================================
    // INITIALIZATION
    // =========================================================

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


    // =========================================================
    // EXECUTE MELEE
    // =========================================================

    public override bool Execute(
        Transform caster,
        Transform targetAnchor,
        bool isHolding)
    {
        if (caster == null)
        {
            Debug.Log("[MELEE BLOCKED] Caster is null");
            return false;
        }

        if (!isHolding)
        {
            Debug.Log("[MELEE BLOCKED] Input is not being held");
            return false;
        }

        if (ActionLock.IsLocked)
        {
            Debug.Log("[MELEE BLOCKED] ActionLock is active");
            return false;
        }

        if (IsOnCooldown)
        {
            Debug.Log(
                $"[MELEE BLOCKED] Final combo cooldown: " +
                $"{cooldownEndTime - Time.time:F3}s remaining"
            );

            return false;
        }

        if (Time.time < nextSwingTime)
        {
            Debug.Log(
                $"[MELEE BLOCKED] Swing frequency cooldown: " +
                $"{nextSwingTime - Time.time:F3}s remaining"
            );

            return false;
        }

        if (BuildManager.Instance != null &&
            BuildManager.Instance.IsPlacing)
        {
            Debug.Log("[MELEE BLOCKED] BuildManager is placing");
            return false;
        }


        // =====================================================
        // DETERMINE OWNER
        // =====================================================

        SwingOwner owner =
            caster.GetComponent<PlayerAttack>() != null
                ? SwingOwner.Player
                : SwingOwner.NPC;


        // =====================================================
        // DETERMINE IF THIS IS THE FINAL COMBO SWING
        // =====================================================

        bool isFinalSwing =
            currentSwingIndex >= maxSwings - 1;


        // =====================================================
        // AUDIO
        // =====================================================

        if (!customAudioLogic &&
            AudioManager.Instance != null &&
            launchsound != null)
        {
            AudioManager.Instance.PlaySound(
                launchsound,
                1f
            );
        }


        // =====================================================
        // CREATE SWING
        // =====================================================

        meleebehav swing = PerformSwing(
            caster,
            targetAnchor,
            currentSwingIndex,
            owner
        );

        if (swing == null)
        {
            Debug.Log(
                "[MELEE] PerformSwing failed - no swing created"
            );

            return false;
        }


        // =====================================================
        // FINAL SWING COOLDOWN
        // =====================================================

        if (isFinalSwing)
        {
            /*
             * IMPORTANT:
             *
             * Do NOT start the cooldown here.
             *
             * The cooldown begins when the melee animation
             * actually finishes.
             */

            swing.OnSwingFinished += OnFinalSwingFinished;

            currentSwingIndex = 0;
        }
        else
        {
            currentSwingIndex++;
        }


        // =====================================================
        // SWING TIMER
        // =====================================================

        nextSwingTime =
            Time.time + swingFreq;


        // =====================================================
        // DEBUG
        // =====================================================

        if (isFinalSwing)
        {
            Debug.Log(
                "[MELEE] Final swing started. " +
                "Cooldown will begin when animation finishes."
            );
        }
        else
        {
            Debug.Log(
                $"[MELEE] Swing {currentSwingIndex} executed. " +
                $"Next swing available in {swingFreq:F2}s"
            );
        }


        // Return true because the swing was successfully created.
        return true;
    }


    // =========================================================
    // FINAL SWING FINISHED
    // =========================================================

    private void OnFinalSwingFinished(bool didHitSomething)
    {
        cooldownEndTime =
            Time.time + fireRate;

        Debug.Log(
            $"[MELEE] Final swing animation finished. " +
            $"Cooldown started: {fireRate:F2}s"
        );
    }


    // =========================================================
    // CREATE SWING
    // =========================================================

    public meleebehav PerformSwing(
        Transform caster,
        Transform targetAnchor,
        int index,
        SwingOwner owner)
    {
        if (prefab == null)
        {
            Debug.Log(
                "[MELEE] Cannot create swing: prefab is null"
            );

            return null;
        }

        if (ObjectPooler.Instance == null)
        {
            Debug.Log(
                "[MELEE] Cannot create swing: ObjectPooler is null"
            );

            return null;
        }


        // =====================================================
        // TARGET POSITION
        // =====================================================

        Vector3 targetPos =
            targetAnchor != null
                ? targetAnchor.position
                : caster.position + caster.right;


        // =====================================================
        // DIRECTION
        // =====================================================

        Vector2 dir =
            (
                (Vector2)targetPos -
                (Vector2)caster.position
            ).normalized;


        // =====================================================
        // NORTH / SOUTH ONLY
        // =====================================================

        Vector2 snappedDir;

        if (Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))
        {
            snappedDir =
                new Vector2(
                    0f,
                    Mathf.Sign(dir.y)
                );
        }
        else
        {
            snappedDir =
                new Vector2(
                    0f,
                    dir.y >= 0f ? 1f : -1f
                );
        }


        // =====================================================
        // POSITION / ROTATION
        // =====================================================

        Vector3 offset =
            (Vector3)(snappedDir * spawnOffset);

        float angle =
            Mathf.Atan2(
                snappedDir.y,
                snappedDir.x
            ) * Mathf.Rad2Deg
            + rotationOffset;


        // =====================================================
        // GET POOLED OBJECT
        // =====================================================

        GameObject woosh =
            ObjectPooler.Instance.GetPooledObject(
                prefab,
                caster.position + offset,
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                )
            );

        if (woosh == null)
        {
            Debug.Log(
                "[MELEE] ObjectPooler returned null"
            );

            return null;
        }

        woosh.SetActive(true);


        // =====================================================
        // BONUS DAMAGE
        // =====================================================

        int bonus =
            owner == SwingOwner.Player
                ? Mathf.RoundToInt(
                    GetBonusDamage()
                )
                : 0;


        // =====================================================
        // GET MELEE BEHAVIOUR
        // =====================================================

        meleebehav behav =
            woosh.GetComponent<meleebehav>();

        if (behav == null)
        {
            Debug.LogError(
                "[MELEE] Pooled melee prefab has no " +
                "meleebehav component!"
            );

            woosh.SetActive(false);

            return null;
        }


        // =====================================================
        // SETUP SWING
        // =====================================================

        behav.Setup(
            damage,
            bonus,
            index,
            owner,
            caster,
            offset,
            (
                owner == SwingOwner.Player &&
                this is ToolsSO toolSO
            )
                ? toolSO.toolType
                : ToolType.None
        );


        // =====================================================
        // PLAYER CAMERA SHAKE
        // =====================================================

        if (owner == SwingOwner.Player &&
            CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(
                0.15f,
                0.1f
            );
        }


        return behav;
    }
}