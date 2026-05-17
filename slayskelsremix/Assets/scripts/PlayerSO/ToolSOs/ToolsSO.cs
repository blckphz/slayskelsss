using UnityEngine;

[CreateAssetMenu(fileName = "NewToolAbility", menuName = "Abilities/Tools/BaseTool")]
public class ToolsSO : offensivemelee
{
    [Header("Tool Configuration")]
    [Tooltip("The main tool identity classification used by harvesting targets.")]
    public ToolType toolType;
}