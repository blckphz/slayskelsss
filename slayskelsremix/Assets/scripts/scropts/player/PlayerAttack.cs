using System.Collections.Generic;

using UnityEngine;

using UnityEngine.InputSystem;



public class PlayerAttack : MonoBehaviour

{

    public charSO currentChar;

    public PlayerAim aimScript;

    public InputActionReference[] fireActions;

    public Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();



    void Update()

    {

        if (currentChar == null || currentChar.abilities == null) return;



        for (int i = 0; i < fireActions.Length; i++)

        {

            if (i >= currentChar.abilities.Length) break;

            HandleInput(fireActions[i], i);

        }

    }



    private void HandleInput(InputActionReference actionRef, int index)

    {

        if (actionRef == null || actionRef.action == null) return;



        Ability ability = currentChar.abilities[index];

        bool isHeld = actionRef.action.IsPressed();



        float cdTimestamp = GetCooldown(ability);

        bool isWeaponReady = Time.time >= cdTimestamp;



        if (ability is offensivemelee melee)

        {

            // Execute now returns TRUE if the maxSwings limit is reached

            bool comboFinished = melee.Execute(transform, aimScript.anchor, isHeld && isWeaponReady);



            // Apply cooldown if:

            // 1. The combo is naturally finished (even if button is still held)

            // 2. OR the user released the button

            if (isWeaponReady && (comboFinished || actionRef.action.WasReleasedThisFrame()))

            {

                ApplyCooldown(ability);

            }

        }

        else

        {

            // Ranged path

            if (isHeld && isWeaponReady)

            {

                ability.Execute(transform, aimScript.anchor, true);

                ApplyCooldown(ability);

            }

        }

    }



    private float GetCooldown(Ability ability)

    {

        if (!abilityCooldowns.ContainsKey(ability)) abilityCooldowns[ability] = 0f;

        return abilityCooldowns[ability];

    }



    private void ApplyCooldown(Ability ability)

    {

        abilityCooldowns[ability] = Time.time + ability.fireRate;

        if (charsetter.Instance != null) charsetter.Instance.TriggerAbilityUsed(ability);

    }

}