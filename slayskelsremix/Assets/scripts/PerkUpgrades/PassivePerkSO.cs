using UnityEngine;

public abstract class PassivePerkSO : AbilityUpgradeSO
{
    public override void Apply(Ability ability)
    {
        ApplyPassive();
    }

    public abstract void ApplyPassive();
    public abstract void RemovePassive();
}