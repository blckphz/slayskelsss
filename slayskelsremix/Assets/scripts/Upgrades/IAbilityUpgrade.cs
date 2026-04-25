public interface IAbilityUpgrade
{
    string UpgradeName { get; }
    string Description { get; }
    int Level { get; set; }
    int MaxLevel { get; }
    void Apply(Ability ability);
}