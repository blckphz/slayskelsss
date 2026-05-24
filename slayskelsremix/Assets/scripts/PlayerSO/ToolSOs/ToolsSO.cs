using UnityEngine;

[CreateAssetMenu(fileName = "NewToolAbility", menuName = "Abilities/Tools/BaseTool")]
public class ToolsSO : offensivemelee
{
    [Header("Tool Configuration")]
    [Tooltip("The main tool identity classification used by harvesting targets.")]
    public ToolType toolType;

    // We override Execute to ensure Tools maintain their identity while respecting the base lock
    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        return base.Execute(caster, targetAnchor, isHolding);
    }
}