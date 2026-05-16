using UnityEngine;

[CreateAssetMenu(fileName = "NewShovelAbility", menuName = "Abilities/Tools/Shovel")]
public class ShowelSO : ToolsSO
{
    [Header("Digging")]
    public buildSO holeBuild;

    public override bool ExecuteSecondary(Transform caster)
    {
        Debug.Log($"<color=yellow>[Shovel Ability]</color> ExecuteSecondary triggered on {name}. Checking requirements.");

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

        Debug.Log($"[Shovel Ability SUCCESS] Dispatching build setup command to BuildManager for asset object: {holeBuild.name}");
        BuildManager.Instance.StartPlacing(holeBuild);
        return true;
    }
}