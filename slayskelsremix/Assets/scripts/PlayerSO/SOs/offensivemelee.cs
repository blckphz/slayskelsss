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
    [Header("Runtime Debug View")]
    [SerializeField] private int swingIndex;
    [SerializeField] private float nextSwingTime;
    [SerializeField] private bool isActive;

    // Fixes the ScriptableObject caching data bug between play sessions
    private void OnEnable()
    {
        Debug.Log($"<color=cyan>[Melee Asset Reset]</color> {name} initialized. Clearing lingering variables.");
        ResetMeleeState();
    }

    public void ResetMeleeState()
    {
        isActive = false;
        swingIndex = 0;
        nextSwingTime = 0f;
    }

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (caster == null)
        {
            Debug.LogWarning("[Melee CRITICAL] Execute failed: caster is NULL!");
            return false;
        }

        SwingOwner owner = caster.GetComponent<PlayerAttack>() != null
            ? SwingOwner.Player
            : SwingOwner.NPC;

        // 1. Handle Combo Initialization
        if (isHolding && !isActive)
        {
            isActive = true;
            swingIndex = 0;
            nextSwingTime = Time.time;

            Debug.Log($"<color=green>[Melee COMBO START]</color> Owner: {owner} | ScriptableObject: {name}");
        }

        // 2. Handle Button Release / Stop Execution
        if (!isHolding)
        {
            if (isActive)
            {
                Debug.Log($"<color=orange>[Melee COMBO INTERRUPTED]</color> Input button released. Cleaning state.");
                ResetMeleeState();
            }
            return false;
        }

        // 3. Gatekeeper Diagnostics
        if (!isActive)
        {
            Debug.LogWarning($"[Melee BLOCKED] Execution rejected. 'isActive' is false while holding mouse button.");
            return false;
        }

        if (Time.time < nextSwingTime)
        {
            Debug.Log($"[Melee COOLDOWN] Suppressing swing. Wait time remaining: {nextSwingTime - Time.time:F2}s");
            return false;
        }

        // 4. Proceed to swing execution
        Debug.Log($"<color=yellow>[Melee PROCEEDING]</color> Processing swing {swingIndex + 1}/{maxSwings} for {owner}");

        PerformSwing(caster, targetAnchor, swingIndex, owner);

        swingIndex++;
        nextSwingTime = Time.time + swingFreq;

        // 5. Handle Combo Completion
        if (swingIndex >= maxSwings)
        {
            Debug.Log("<color=magenta>[Melee COMBO FINISHED]</color> Max combo threshold reached. Resetting.");
            ResetMeleeState();
            return true;
        }

        return false;
    }

    private void PerformSwing(Transform caster, Transform targetAnchor, int index, SwingOwner owner)
    {
        if (prefab == null)
        {
            Debug.LogError($"[Melee CRITICAL] '{name}' has NO PREFAB assigned in the inspector!");
            return;
        }

        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("[Melee CRITICAL] ObjectPooler.Instance is completely missing from your active scene layers!");
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

        Debug.Log($"[Melee SPAWN TRY] Pulling '{prefab.name}' from Pooler at: {spawnPos} (Angle: {angle}°)");

        GameObject woosh = ObjectPooler.Instance.GetPooledObject(
            prefab,
            spawnPos,
            Quaternion.Euler(0, 0, angle)
        );

        if (woosh == null)
        {
            Debug.LogError($"[Melee POOL FAILURE] ObjectPooler returned NULL for {prefab.name}. Is your pool size limit too small?");
            return;
        }

        if (!woosh.activeInHierarchy)
        {
            Debug.LogWarning($"[Melee POOL WARNING] Item pooled but returned in an INACTIVE state. Forcing active.");
            woosh.SetActive(true);
        }

        float bonus = owner == SwingOwner.Player ? GetBonusDamage() : 0f;

        Debug.Log($"[Melee SETUP RUN] Damage: {damage} | Bonus: {bonus} | Target Frame: {index}");

        var behav = woosh.GetComponent<meleebehav>();
        if (behav != null)
        {
            behav.Setup(damage, bonus, index, owner);
            Debug.Log("[Melee SETUP SUCCESS] meleebehav configuration assigned successfully.");
        }
        else
        {
            Debug.LogError($"[Melee COMPONENT ERROR] The prefab '{woosh.name}' does not contain a 'meleebehav' component script!");
        }

        if (owner == SwingOwner.Player && CameraShaker.Instance != null)
        {
            Debug.Log("[Melee EFFECTS] Camera shake executed.");
            CameraShaker.Instance.Shake(0.15f, 0.1f);
        }
    }

    public string GetStatsFormat()
    {
        return $"Combo Swings: {maxSwings}\n" +
               $"Swing Speed: {swingFreq}s";
    }
}