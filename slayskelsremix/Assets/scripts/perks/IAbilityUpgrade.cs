public interface IAbilityUpgrade
{
    string UpgradeName { get; }
    string Description { get; } // Added this so the UI can read it
    void Apply(Ability ability);
}