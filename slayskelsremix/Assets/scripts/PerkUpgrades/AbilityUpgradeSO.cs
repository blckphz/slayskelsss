using UnityEngine;


public enum PerkType
{
    AbilityUpgrade,
    Passive
}

public abstract class AbilityUpgradeSO : ScriptableObject, IAbilityUpgrade
{
    [Header("Perk Type")]
    public PerkType perkType = PerkType.AbilityUpgrade;

    [Header("Level 0 (Unlock) Display")]
    public string perkName;
    [TextArea] public string perkdescription;

    [Header("Level 1+ (Upgrade) Display")]
    public string upgradeName;
    [TextArea] public string description;

    [Header("Leveling Settings")]
    public int currentLevel = 0;
    public int maxLevel = 5;

    [Header("Ability Modification")]
    public bool givesNewAbility = false;
    public Ability newAbility;

    public string UpgradeName => currentLevel <= 0 ? perkName : upgradeName;
    public string Description => currentLevel <= 0 ? perkdescription : description;

    public int Level
    {
        get => currentLevel;
        set => currentLevel = value;
    }

    public int MaxLevel => maxLevel;

    public abstract void Apply(Ability ability);

    public (string displayName, string displayDesc) GetDisplayStrings()
    {
        if (currentLevel <= 0)
            return (perkName, perkdescription);

        return (upgradeName, description);
    }

    public void SaveLevel()
    {
        PlayerPrefs.SetInt(name + "_SavedLevel", currentLevel);
        PlayerPrefs.Save();
    }

    public void LoadLevel()
    {
        currentLevel = PlayerPrefs.GetInt(name + "_SavedLevel", 0);
    }

    public void ResetLevel()
    {
        currentLevel = 0;
        PlayerPrefs.DeleteKey(name + "_SavedLevel");
    }
}