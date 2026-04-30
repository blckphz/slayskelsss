using UnityEngine;

public abstract class AbilityUpgradeSO : ScriptableObject, IAbilityUpgrade
{
    [Header("UI Display")]
    public string upgradeName;
    [TextArea] public string description;

    public string perkName;
    [TextArea] public string perkdescription;

    [Header("Leveling Settings")]
    public int currentLevel = 0;
    public int maxLevel = 5;

    public bool givesNewAbility = false; // Set to true if this upgrade grants a new ability
    public Ability newAbility; // The new ability granted by this upgrade (if applicable)



    public string UpgradeName => upgradeName;
    public string Description => description;
    public int Level { get => currentLevel; set => currentLevel = value; }
    public int MaxLevel => maxLevel;

    public abstract void Apply(Ability ability);

    // Saves the level to disk using the Asset's name as a unique key
    public void SaveLevel()
    {
        PlayerPrefs.SetInt(this.name + "_SavedLevel", currentLevel);
        PlayerPrefs.Save();
    }

    // Loads the level from disk
    public void LoadLevel()
    {
        currentLevel = PlayerPrefs.GetInt(this.name + "_SavedLevel", 0);
    }

    // Call this to completely wipe progress for this specific perk
    public void ResetLevel()
    {
        currentLevel = 0;
        PlayerPrefs.DeleteKey(this.name + "_SavedLevel");
    }
}