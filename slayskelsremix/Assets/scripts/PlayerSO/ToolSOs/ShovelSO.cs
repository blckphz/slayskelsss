using UnityEngine;

[CreateAssetMenu(fileName = "NewShovelAbility", menuName = "Abilities/Tools/Shovel")]
public class ShovelSO : ToolsSO, IItemDescriptionProvider
{
    [Header("Digging")]
    public buildSO holeBuild;

    // ✅ Now valid because base is virtual
    public override string GetDetailedDescription()
    {
        return $"<color=#FF6B6B>Damage:</color> {damage}\n" +
               $"<color=#A2E8DD>Swing Rate:</color> {fireRate}s\n" +
               "------------------\n" +
               "<color=#FFA500><b>[TOOL TYPE]</b></color>\n" +
               $"• {toolType}\n\n" +
               "<color=#FFA500><b>[PRIMARY ACTION]</b></color>\n" +
               "• Swing or place flooring when in build modus.\n\n" +
               "<color=#FFA500><b>[SECONDARY ACTION]</b></color>\n" +
               "• Dig Hole: Toggle placement mode to create a new planting spot.";
    }

    public override bool ExecuteSecondary(Transform caster)
    {

        if (!BuildState.IsBuildMode)
            return false;

        if (holeBuild == null || BuildManager.Instance == null)
            return false;

        // Toggle Logic: Cancel if already placing this item
        if (BuildManager.Instance.IsPlacing &&
            BuildManager.Instance.GetCurrentItem() == holeBuild)
        {
            BuildManager.Instance.Cancel();
        }
        else
        {
            BuildManager.Instance.StartPlacing(holeBuild);
        }

        return true;
    }
}