using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Inventory/Ability Database")]
public class AbilityDatabase : ScriptableObject
{
    public List<Ability> allAbilities = new List<Ability>();

    // Finds an ability by its ScriptableObject name
    public Ability GetAbilityByName(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName)) return null;

        foreach (var ability in allAbilities)
        {
            if (ability.name == abilityName)
            {
                return ability;
            }
        }

        Debug.LogWarning($"[AbilityDatabase] Ability '{abilityName}' not found in database!");
        return null;
    }
}