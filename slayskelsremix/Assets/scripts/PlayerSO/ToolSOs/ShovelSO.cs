using UnityEngine;

[CreateAssetMenu(fileName = "NewShovelAbility", menuName = "Abilities/Tools/Shovel")]
public class ShowelSO : ToolsSO, IItemDescriptionProvider
{
    [Header("Digging")]
    public buildSO holeBuild;

    // ✅ MODULAR INTERFACE WITH STATS & DESCRIPTION
    public string GetDetailedDescription()
    {
        string lines = "";

        // Shovel doesn't use damage variables, so display just the tool layout use rate
        lines += $"<color=#A2E8DD>Use Rate:</color> {fireRate}s\n";
        lines += "-----------------------------\n";

        lines += "<color=#FFA500><b>[PRIMARY ACTION]</b></color>\n";
        lines += "• <color=#FF6B6B>Swing:</color> Swing tool forward to clear debris or attack.\n\n";

        lines += "<color=#FFA500><b>[SECONDARY ACTION]</b></color>\n";
        lines += "• <color=#A2E8DD>Dig:</color> Places a layout target to dig a farming hole.\n\n";

        return lines;
    }

    public override bool ExecuteSecondary(Transform caster)
    {
        if (holeBuild == null)
        {
            Debug.LogWarning("[Shovel Ability] Secondary execution aborted: 'holeBuild' buildSO reference has not been assigned in inspector.");
            return false;
        }

        if (BuildManager.Instance == null)
        {
            Debug.LogError("[Shovel Ability] Secondary execution aborted: BuildManager instance is completely missing from this scene layout context.");
            return false;
        }

        BuildManager.Instance.StartPlacing(holeBuild);
        return true;
    }
}