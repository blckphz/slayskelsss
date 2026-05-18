using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAbility", menuName = "Abilities/Melee")]
public class offensivemelee : offensiveability
{
    public enum SwingOwner
    {
        Player,
        NPC
    }

    [Tooltip("Check this if the ability handles its own sound timing (like melee combos).")]
    public bool customAudioLogic = false;

    [Header("Melee Stats")]
    public int maxSwings = 3;
    public float swingFreq = 0.2f; // Fast window gap between inner combo swings

    [Header("Spawn")]
    public float spawnOffset = 1.5f;
    public float rotationOffset = 0f;

    private int currentSwingIndex = 0;

    public virtual float GetBonusDamage() => 0f;

    public void ResetMeleeState()
    {
        currentSwingIndex = 0;
    }

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (caster == null || !isHolding) return false;

        SwingOwner owner = caster.GetComponent<PlayerAttack>() != null
            ? SwingOwner.Player
            : SwingOwner.NPC;

        // Perform attack using current tracking index
        PerformSwing(caster, targetAnchor, currentSwingIndex, owner);

        currentSwingIndex++;

        // If the combo has arrived at its final swing limit
        if (currentSwingIndex >= maxSwings)
        {
            currentSwingIndex = 0; // Clear for next time
            return true; // 🌟 Returns TRUE: Signals PlayerAttack to run the big fireRate cooldown!
        }

        return false; // 🌟 Returns FALSE: Tell PlayerAttack to just use the shorter swingFreq spacer
    }

    public void PerformSwing(Transform caster, Transform targetAnchor, int index, SwingOwner owner)
    {
        if (prefab == null || ObjectPooler.Instance == null) return;

        Vector3 targetPos = targetAnchor != null ? targetAnchor.position : caster.position + caster.right;
        Vector2 dir = ((Vector2)targetPos - (Vector2)caster.position).normalized;

        Vector2 snappedDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0)
            : new Vector2(0, Mathf.Sign(dir.y));

        Vector3 localSpawnOffset = (Vector3)(snappedDir * spawnOffset);
        Vector3 globalSpawnPos = caster.position + localSpawnOffset;

        float angle = Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg + rotationOffset;

        GameObject woosh = ObjectPooler.Instance.GetPooledObject(prefab, globalSpawnPos, Quaternion.Euler(0, 0, angle));
        if (woosh == null) return;

        if (!woosh.activeInHierarchy) woosh.SetActive(true);

        float bonus = owner == SwingOwner.Player ? GetBonusDamage() : 0f;

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
        return $"Combo Swings: {maxSwings}\n" + $"Swing Speed: {swingFreq}s";
    }
}