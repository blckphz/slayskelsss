using UnityEngine;

[CreateAssetMenu(fileName = "NewToolAbility", menuName = "Abilities/Tools/BaseTool")]
public class ToolsSO : offensivemelee, IItemDescriptionProvider
{
    [Header("Tool Configuration")]
    [Tooltip("The main tool identity classification used by harvesting targets.")]
    public ToolType toolType;


    [Header("Bonus Damage")]
    public int bonusDamage = 0;


    // =====================================================
    // DESCRIPTION
    // =====================================================

    public virtual string GetDetailedDescription()
    {
        return $"<color=#FF6B6B>Damage:</color> {damage}\n" +
               $"<color=#A2E8DD>Swing Rate:</color> {fireRate}s\n" +
               "------------------\n" +
               "<color=#FFA500><b>[TOOL TYPE]</b></color>\n" +
               $"• {toolType}\n\n" +
               "<color=#FFA500><b>[PRIMARY ACTION]</b></color>\n" +
               "• Swing: Used to harvest resources and deal melee damage.";
    }

    // =====================================================
    // OPTIONAL EXECUTE HOOK (if your system uses it)
    // =====================================================

    public virtual bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        return true;
    }
}