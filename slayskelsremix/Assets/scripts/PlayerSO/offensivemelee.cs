using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAbility", menuName = "Abilities/Melee")]
public class offensivemelee : offensiveability
{
    // 🔥 Enum for owner detection
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

    // Runtime state (per instance)
    private int swingIndex;
    private float nextSwingTime;
    private bool isActive;

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (caster == null)
            return false;

        // Determine if the caster is the player or an NPC
        SwingOwner owner = caster.GetComponent<PlayerAttack>() != null
            ? SwingOwner.Player
            : SwingOwner.NPC;

        // Start the combo sequence
        if (isHolding && !isActive)
        {
            isActive = true;
            swingIndex = 0;
            nextSwingTime = Time.time;
        }

        // Stop if button released
        if (!isHolding)
        {
            isActive = false;
            return false;
        }

        // Wait for cooldown between swings
        if (!isActive || Time.time < nextSwingTime)
            return false;

        PerformSwing(caster, targetAnchor, swingIndex, owner);

        swingIndex++;
        nextSwingTime = Time.time + swingFreq;

        // End combo if max swings reached
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

        // Calculate direction and rotation
        Vector3 targetPos = targetAnchor != null
            ? targetAnchor.position
            : caster.position + caster.right;

        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;

        // Snap to 4-way grid (Optional logic based on your previous code)
        Vector2 snappedDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0)
            : new Vector2(0, Mathf.Sign(dir.y));

        Vector3 spawnPos = caster.position + (Vector3)(snappedDir * spawnOffset);
        float angle = Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg + rotationOffset;

        // Get the swing prefab from the pool
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

        // Parenting and setup
        woosh.transform.SetParent(caster);
        woosh.transform.localPosition = caster.InverseTransformPoint(spawnPos);

        float bonus = 0f;
        if (owner == SwingOwner.Player)
        {
            bonus = GetBonusDamage();
        }

        // Setup the behavior on the instantiated prefab
        var behav = woosh.GetComponent<meleebehav>();
        if (behav != null)
        {
            behav.Setup(damage, bonus, index, owner);
        }

        // --- SCREEN SHAKE (SWING FEEDBACK) ---
        // We only want the camera to shake when the player swings.
        if (owner == SwingOwner.Player && CameraShaker.Instance != null)
        {
            // 0.15 intensity is enough for a "whoosh"
            // The meleebehav script handles the stronger 0.35 shake when hitting an enemy.
            CameraShaker.Instance.Shake(0.15f, 0.1f);
        }
    }

    public string GetStatsFormat()
    {
        return $"Combo Swings: {maxSwings}\n" +
               $"Swing Speed: {swingFreq}s";
    }
}