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

    private void OnEnable()
    {
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
        }

        // 2. Handle Button Release / Stop Execution
        if (!isHolding)
        {
            if (isActive)
            {
                ResetMeleeState();
            }
            return false;
        }

        // 3. Gatekeeper Diagnostics
        if (!isActive)
        {
            return false;
        }

        if (Time.time < nextSwingTime)
        {
            return false;
        }

        // 4. Proceed to swing execution
        PerformSwing(caster, targetAnchor, swingIndex, owner);

        swingIndex++;
        nextSwingTime = Time.time + swingFreq;

        // 5. Handle Combo Completion
        if (swingIndex >= maxSwings)
        {
            ResetMeleeState();
            return true;
        }

        return true;
    }

    private void PerformSwing(Transform caster, Transform targetAnchor, int index, SwingOwner owner)
    {
        if (prefab == null || ObjectPooler.Instance == null)
        {
            return;
        }

        Vector3 targetPos = targetAnchor != null
            ? targetAnchor.position
            : caster.position + caster.right;

        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;

        Vector2 snappedDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0)
            : new Vector2(0, Mathf.Sign(dir.y));

        // Calculate offset locally relative to caster
        Vector3 localSpawnOffset = (Vector3)(snappedDir * spawnOffset);
        Vector3 globalSpawnPos = caster.position + localSpawnOffset;

        float angle = Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg + rotationOffset;

        GameObject woosh = ObjectPooler.Instance.GetPooledObject(
            prefab,
            globalSpawnPos,
            Quaternion.Euler(0, 0, angle)
        );

        if (woosh == null)
        {
            return;
        }

        if (!woosh.activeInHierarchy)
        {
            woosh.SetActive(true);
        }

        float bonus = owner == SwingOwner.Player ? GetBonusDamage() : 0f;

        // Determine tool type: Use the ToolsSO property if this asset is a tool, otherwise fallback to Axe/Default
        ToolType activeTool = ToolType.Axe;
        if (this is ToolsSO toolAbility)
        {
            activeTool = toolAbility.toolType;
        }

        var behav = woosh.GetComponent<meleebehav>();
        if (behav != null)
        {
            behav.Setup(damage, bonus, index, owner, caster, localSpawnOffset, activeTool);
        }

        if (owner == SwingOwner.Player && CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(0.15f, 0.1f);
        }
    }

    public string GetStatsFormat()
    {
        return $"Combo Swings: {maxSwings}\n" +
               $"Swing Speed: {swingFreq}s";
    }
}