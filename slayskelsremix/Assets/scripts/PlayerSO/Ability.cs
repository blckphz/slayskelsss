using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public float fireRate = 0.2f;
    public GameObject prefab;
    public Sprite icon;
    public AudioClip launchsound;

    [Tooltip("Check this if the ability handles its own sound timing (like melee combos).")]
    public bool customAudioLogic = false;

    // UPDATE: Added 'bool isHolding' so all child classes can see the input state
    public abstract void Execute(Transform caster, Transform targetAnchor, bool isHolding);
}