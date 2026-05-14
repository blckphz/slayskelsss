using UnityEngine;

public class AbilityLoadout : MonoBehaviour
{
    public static AbilityLoadout Instance { get; private set; }

    public Ability[] equippedAbilities = new Ability[4];

    void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public void SetAbility(int index, Ability ability)
    {
        if (index < 0 || index >= equippedAbilities.Length) return;
        equippedAbilities[index] = ability;

        if (charsetter.Instance != null)
            charsetter.Instance.UpdateAbilityIcons();
    }

    public Ability GetAbility(int index)
    {
        if (index < 0 || index >= equippedAbilities.Length) return null;
        return equippedAbilities[index];
    }
}