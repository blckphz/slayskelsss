using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public Sprite icon;
    public AudioClip launchsound;
    public float fireRate = 0.2f;
    public GameObject prefab;
  




    // Primary Use (Throwing)
    public abstract bool Execute(Transform caster, Transform targetAnchor, bool isHolding);

    // Secondary Use (Consuming) - Defaults to false if not overridden
    public virtual bool ExecuteSecondary(Transform caster)
    {
        Debug.LogWarning($"{this.name} does not have a Secondary Use implemented.");
        return false;
    }
}