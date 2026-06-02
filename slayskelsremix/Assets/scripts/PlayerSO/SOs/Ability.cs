using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public string description;
    public Sprite icon;
    public AudioClip launchsound;
    public float fireRate;
    public bool isOnCooldown;
    public GameObject prefab;

    public float staminaUsed;

    private void OnEnable()
    {
        ResetRuntimeState();
    }

    protected virtual void ResetRuntimeState()
    {
        isOnCooldown = false;
    }

    // =====================================================
    // AUDIO
    // =====================================================

    protected void PlaySound()
    {
        if (launchsound == null)
            return;

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager instance not found in scene.");
            return;
        }

        AudioManager.Instance.PlaySound(launchsound);
    }

    // =====================================================
    // EXECUTION
    // =====================================================

    public abstract bool Execute(Transform caster, Transform targetAnchor, bool isHolding);

    public virtual bool ExecuteSecondary(Transform caster)
    {
        Debug.LogWarning($"{name} does not have a Secondary Use implemented.");
        return false;
    }
}