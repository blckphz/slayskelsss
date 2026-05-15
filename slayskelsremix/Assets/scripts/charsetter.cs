using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class charsetter : MonoBehaviour
{
    public static charsetter Instance { get; private set; }

    [Header("References")]
    public AbilityUI[] abilityUI; // The 4 Hotbar UI Slot objects
    public PlayerAttack playerAttack;

    private Dictionary<Ability, float> abilityUsedTime = new Dictionary<Ability, float>();

    [Header("Chain UI Settings")]
    public TextMeshProUGUI chargeText;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;

    private int lastChargeValue = -1;
    private Coroutine chargePopRoutine;
    private Coroutine chargeShakeRoutine;
    private bool uiInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Initial setup
        UpdateAbilityIcons();
        ForceChargeUIRefresh();
        uiInitialized = true;
    }

    void Update()
    {
        UpdateCooldownUI();
        UpdateChargeUI();
    }

    /// <summary>
    /// Synchronizes the Hotbar icons with the current AbilityLoadout.
    /// </summary>
    public void UpdateAbilityIcons()
    {
        if (AbilityLoadout.Instance == null) return;

        for (int i = 0; i < abilityUI.Length; i++)
        {
            if (abilityUI[i] == null) continue;

            Ability ability = AbilityLoadout.Instance.GetAbility(i);

            if (ability != null)
            {
                // Set Main Icon
                if (abilityUI[i].icon != null)
                {
                    abilityUI[i].icon.sprite = ability.icon;
                    abilityUI[i].icon.enabled = true;
                    // Reset fill for cooldowns
                    abilityUI[i].icon.fillAmount = 0;
                }

                // Set Background (if you use a blurry/shadow version of the icon)
                Transform bgTransform = abilityUI[i].transform.Find("Background");
                if (bgTransform != null && bgTransform.TryGetComponent(out Image bg))
                {
                    bg.sprite = ability.icon;
                    bg.enabled = true;
                }

                // Track used time for internal logic
                if (!abilityUsedTime.ContainsKey(ability))
                    abilityUsedTime[ability] = -Mathf.Infinity;
            }
            else
            {
                // Disable visuals if slot is empty
                if (abilityUI[i].icon != null)
                    abilityUI[i].icon.enabled = false;

                Transform bgTransform = abilityUI[i].transform.Find("Background");
                if (bgTransform != null && bgTransform.TryGetComponent(out Image bg))
                    bg.enabled = false;
            }
        }
    }

    public void TriggerAbilityUsed(Ability ability)
    {
        if (ability == null) return;
        abilityUsedTime[ability] = Time.time;
    }

    private void UpdateCooldownUI()
    {
        if (AbilityLoadout.Instance == null || playerAttack == null) return;

        for (int i = 0; i < abilityUI.Length; i++)
        {
            if (abilityUI[i] == null) continue;

            Ability ability = AbilityLoadout.Instance.GetAbility(i);
            if (ability == null || abilityUI[i].icon == null) continue;

            // Check the cooldown from PlayerAttack
            float nextFireTime = playerAttack.abilityCooldowns.ContainsKey(ability)
                ? playerAttack.abilityCooldowns[ability]
                : 0f;

            float remaining = Mathf.Clamp(nextFireTime - Time.time, 0f, ability.fireRate);

            // fillAmount 1 = Cooldown Active, 0 = Ready
            abilityUI[i].icon.fillAmount = (ability.fireRate > 0) ? (remaining / ability.fireRate) : 0;
        }
    }

    // --- CHARGE / CHAIN UI LOGIC ---

    public void ForceChargeUIRefresh()
    {
        if (chargeText == null) return;
        lastChargeValue = -1;

        if (chainController.isUnlocked)
        {
            chargeText.enabled = true;
            chargeText.text = chainController.hitCounter.ToString();
        }
        else
        {
            chargeText.enabled = false;
        }
    }

    private void UpdateChargeUI()
    {
        if (chargeText == null || !uiInitialized || !chainController.isUnlocked) return;

        int charges = chainController.hitCounter;
        chargeText.text = charges.ToString();

        if (charges > lastChargeValue)
        {
            if (chargePopRoutine != null) StopCoroutine(chargePopRoutine);
            chargePopRoutine = StartCoroutine(ChargePop());
        }
        else if (charges < lastChargeValue)
        {
            if (chargeShakeRoutine != null) StopCoroutine(chargeShakeRoutine);
            chargeShakeRoutine = StartCoroutine(ChargeShake());
        }

        lastChargeValue = charges;
        chargeText.color = charges <= 0 ? Color.gray : (charges < 5 ? Color.white : Color.yellow);
    }

    private IEnumerator ChargePop()
    {
        RectTransform rt = chargeText.rectTransform;
        float duration = 0.15f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.one * Mathf.Lerp(1f, 1.4f, t / duration);
            yield return null;
        }
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, t / duration);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private IEnumerator ChargeShake()
    {
        RectTransform rt = chargeText.rectTransform;
        Vector2 originalPos = rt.anchoredPosition;
        float time = 0f;
        while (time < 0.12f)
        {
            time += Time.deltaTime;
            rt.anchoredPosition = originalPos + Random.insideUnitCircle * Mathf.Lerp(4f, 0f, time / 0.12f);
            yield return null;
        }
        rt.anchoredPosition = originalPos;
    }
}