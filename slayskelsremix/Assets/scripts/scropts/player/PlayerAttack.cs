using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 1. Added namespace

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public charSO currentChar;
    public PlayerAim aimScript; // 2. Direct reference is much faster than FindObjectOfType

    [Header("Input Actions")]
    // 3. Define your actions here
    public InputActionReference fire1;
    public InputActionReference fire2;
    public InputActionReference fire3;
    public InputActionReference fire4;

    [Header("Settings")]
    [SerializeField] private float defaultShakeIntensity = 0.5f;

    public Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    // Optimization: Get the anchor once or via direct reference
    public Transform CurrentAnchor => (aimScript != null) ? aimScript.anchor : null;

    private void OnEnable()
    {
        // 4. Must enable actions to use them
        fire1.action.Enable();
        fire2.action.Enable();
        if (fire3 != null) fire3.action.Enable();
        if (fire4 != null) fire4.action.Enable();
    }

    void Update()
    {
        if (currentChar == null || currentChar.abilities == null || currentChar.abilities.Length == 0)
            return;

        // 5. Use .IsPressed() or .WasPressedThisFrame() instead of GetButton
        if (fire1.action.IsPressed()) TryUseAbility(0);

        if (currentChar.abilities.Length > 1 && fire2.action.IsPressed())
            TryUseAbility(1);

        if (currentChar.abilities.Length > 2 && fire3 != null && fire3.action.IsPressed())
            TryUseAbility(2);

        if (currentChar.abilities.Length > 3 && fire4 != null && fire4.action.IsPressed())
            TryUseAbility(3);
    }

    private void TryUseAbility(int index)
    {
        if (index >= currentChar.abilities.Length) return;

        Ability ability = currentChar.abilities[index];
        if (ability != null && CanUseAbility(ability))
        {
            PerformAttack(ability);
        }
    }

    private bool CanUseAbility(Ability ability)
    {
        if (!abilityCooldowns.ContainsKey(ability))
            abilityCooldowns[ability] = 0f;

        return Time.time >= abilityCooldowns[ability];
    }

    private void PerformAttack(Ability ability)
    {
        Transform activeAnchor = CurrentAnchor;

        if (activeAnchor == null)
        {
            Debug.LogError($"[PlayerAttack] No Anchor found!");
            return;
        }

        // Logic
        audiomanager.Instance.PlaySound(ability.launchsound);
        ability.Execute(transform, activeAnchor);

        // Cooldown
        abilityCooldowns[ability] = Time.time + ability.fireRate;

        // UI
        if (charsetter.Instance != null)
            charsetter.Instance.TriggerAbilityUsed(ability);
    }
}