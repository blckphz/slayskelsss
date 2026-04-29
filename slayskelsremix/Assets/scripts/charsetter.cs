using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class charsetter : MonoBehaviour
{
    public static charsetter Instance { get; private set; }

    [Header("References")]
    public AbilityUI[] abilityUI; // Assign your 4 UI Slot objects here
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
        UpdateAbilityIcons();
        ForceChargeUIRefresh();
        uiInitialized = true;
    }

    void Update()
    {
        UpdateCooldownUI();
        UpdateChargeUI();
    }

    public void UpdateAbilityIcons()
    {
        if (AbilityLoadout.Instance == null) return;

        for (int i = 0; i < abilityUI.Length; i++)
        {
            // --- SKIP EMPTY SLOTS ---
            if (abilityUI[i] == null) continue;

            Ability ability = AbilityLoadout.Instance.GetAbility(i);

            if (ability != null)
            {
                if (abilityUI[i].icon != null)
                {
                    abilityUI[i].icon.sprite = ability.icon;
                    abilityUI[i].icon.enabled = true;
                }

                Image bg = abilityUI[i].transform.Find("Background")?.GetComponent<Image>();
                if (bg != null)
                {
                    bg.sprite = ability.icon;
                    bg.enabled = true;
                }

                if (!abilityUsedTime.ContainsKey(ability))
                    abilityUsedTime[ability] = -Mathf.Infinity;
            }
            else
            {
                if (abilityUI[i].icon != null) abilityUI[i].icon.enabled = false;
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
            // --- SKIP EMPTY SLOTS ---
            if (abilityUI[i] == null) continue;

            Ability ability = AbilityLoadout.Instance.GetAbility(i);
            if (ability == null || abilityUI[i].icon == null) continue;

            float nextFireTime = playerAttack.abilityCooldowns.ContainsKey(ability)
                ? playerAttack.abilityCooldowns[ability]
                : 0f;

            float remaining = Mathf.Clamp(nextFireTime - Time.time, 0f, ability.fireRate);

            abilityUI[i].icon.fillAmount = (ability.fireRate > 0) ? (remaining / ability.fireRate) : 0;
        }
    }

    // --- CHARGE / CHAIN UI LOGIC ---
    public void ForceChargeUIRefresh()
    {
        if (chargeText == null) return;
        lastChargeValue = -1;

        // Note: Ensure chainController is a static class or accessible instance
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