using UnityEngine;

[CreateAssetMenu(fileName = "NewShovelAbility", menuName = "Abilities/Tools/Tool")]
public class ToolsSO : offensivemelee
{
    public enum ToolType
    {
        Shovel,
        Pickaxe,
        Axe,
        Sickle
    }

}