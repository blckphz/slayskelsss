using UnityEngine;

public abstract class AbilityUpgradeSO : ScriptableObject, IAbilityUpgrade
{
    [Header("UI Display")]
    public string upgradeName;
    [TextArea] public string description;

    // Interface implementation
    public string UpgradeName => upgradeName;
    public string Description => description;

    // The logic is still abstract, implemented by each specific perk file
    public abstract void Apply(Ability ability);
}