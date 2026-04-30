using UnityEngine;
using System.Collections.Generic;

public class AbilityUnlocks : MonoBehaviour
{
    public static AbilityUnlocks Instance { get; private set; }

    private HashSet<Ability> unlockedAbilities = new HashSet<Ability>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UnlockAbility(Ability ability)
    {
        if (ability == null) return;

        if (unlockedAbilities.Contains(ability))
        {
            Debug.Log("Ability already unlocked.");
            return;
        }

        unlockedAbilities.Add(ability);

        Debug.Log($"🔓 Unlocked ability: {ability.name}");

        // Optional: notify UI
        // charsetter.Instance.UpdateAbilityIcons();
    }

    public bool IsUnlocked(Ability ability)
    {
        return unlockedAbilities.Contains(ability);
    }

    public List<Ability> GetAllUnlocked()
    {
        return new List<Ability>(unlockedAbilities);
    }
}