using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AbilityUI
{
    public Image background;
    public Image fillImage;

    [HideInInspector] public Vector3 bgOriginalPos;
    [HideInInspector] public Vector3 fillOriginalPos;
}

public class charsetter : MonoBehaviour
{
    public static charsetter Instance { get; private set; }

    public charSO selectedChar;
    public AbilityUI[] abilityUI;
    public PlayerAttack playerAttack;

    private Dictionary<Ability, float> abilityUsedTime = new Dictionary<Ability, float>();

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        /*

        // Save original positions
        foreach (var ui in abilityUI)
        {
            if (ui.background != null)
                ui.bgOriginalPos = ui.background.rectTransform.localPosition;
            if (ui.fillImage != null)
                ui.fillOriginalPos = ui.fillImage.rectTransform.localPosition;
        }

        */
    }

    void Start()
    {
        UpdateAbilityIcons();
    }

    void Update()
    {
        UpdateCooldownUI();
    }

    public void UpdateAbilityIcons()
    {
        if (selectedChar == null || selectedChar.abilities == null) return;

        for (int i = 0; i < abilityUI.Length; i++)
        {
            if (i < selectedChar.abilities.Length && selectedChar.abilities[i] != null)
            {
                Ability ability = selectedChar.abilities[i];

                if (abilityUI[i].fillImage != null)
                {
                    abilityUI[i].fillImage.sprite = ability.icon;
                    abilityUI[i].fillImage.enabled = true;
                    abilityUI[i].fillImage.fillAmount = 0f;
                }

                if (abilityUI[i].background != null)
                {
                    abilityUI[i].background.sprite = ability.icon;
                    abilityUI[i].background.enabled = true;
                }

                if (!abilityUsedTime.ContainsKey(ability))
                    abilityUsedTime[ability] = -Mathf.Infinity;
            }
            else
            {
                if (abilityUI[i].fillImage != null)
                    abilityUI[i].fillImage.enabled = false;
                if (abilityUI[i].background != null)
                    abilityUI[i].background.enabled = false;
            }
        }
    }

    public void TriggerAbilityUsed(Ability ability)
    {
        if (ability == null) return;

        abilityUsedTime[ability] = Time.time;

        int index = System.Array.IndexOf(selectedChar.abilities, ability);
        if (index >= 0 && index < abilityUI.Length)
        {
            // Stop any running shake coroutine for this UI
           // StopCoroutine(ShakeIcon(abilityUI[index]));
           // StartCoroutine(ShakeIcon(abilityUI[index]));
        }
    }

    private void UpdateCooldownUI()
    {
        if (selectedChar == null || selectedChar.abilities == null || playerAttack == null)
            return;

        for (int i = 0; i < abilityUI.Length; i++)
        {
            if (i >= selectedChar.abilities.Length) continue;
            Ability ability = selectedChar.abilities[i];
            if (ability == null || abilityUI[i].fillImage == null) continue;

            float nextFireTime = playerAttack.abilityCooldowns.ContainsKey(ability)
                ? playerAttack.abilityCooldowns[ability]
                : 0f;

            float remaining = Mathf.Clamp(nextFireTime - Time.time, 0f, ability.fireRate);

            // Reverse fill: 1 = just used, 0 = ready
            float fill = remaining > 0f ? remaining / ability.fireRate : 0f;
            abilityUI[i].fillImage.fillAmount = fill;
        }
    }

    /*

    // --- Static shake coroutine ---
    private IEnumerator ShakeIcon(AbilityUI ui)
    {
        float timer = 0f;

        // Save positions at start
        Vector2 bgPos = ui.background != null ? ui.background.rectTransform.anchoredPosition : Vector2.zero;
        Vector2 fillPos = ui.fillImage != null ? ui.fillImage.rectTransform.anchoredPosition : Vector2.zero;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;
            Vector2 offset = Random.insideUnitCircle * shakeMagnitude;

            if (ui.background != null)
                ui.background.rectTransform.anchoredPosition = bgPos + offset;
            if (ui.fillImage != null)
                ui.fillImage.rectTransform.anchoredPosition = fillPos + offset;

            yield return null;
        }

        // Restore original positions
        if (ui.background != null)
            ui.background.rectTransform.anchoredPosition = bgPos;
        if (ui.fillImage != null)
            ui.fillImage.rectTransform.anchoredPosition = fillPos;
    }
    */

    }
