using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public float fireRate = 0.2f;
    public GameObject prefab;
    public Sprite icon;
    public AudioClip launchsound;

    [Tooltip("Check this if the ability handles its own sound timing (like melee combos).")]
    public bool customAudioLogic = false;

    // Change: Returns bool instead of void
    public abstract bool Execute(Transform caster, Transform targetAnchor, bool isHolding);
}