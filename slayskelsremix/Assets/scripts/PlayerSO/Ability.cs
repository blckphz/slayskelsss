using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public float fireRate = 0.2f;
    public GameObject prefab;
    public Sprite icon;
    public AudioClip launchsound;

    [Tooltip("Check this if the ability handles its own sound timing (like melee combos).")]
    public bool customAudioLogic = false;

    // We pass the "parent" Transform so the SO knows where the player is
    public abstract void Execute(Transform caster, Transform targetAnchor);
}