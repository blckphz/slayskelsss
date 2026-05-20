using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class charsetter : MonoBehaviour
{
    public static charsetter Instance { get; private set; }

    [Header("References")]
    public AbilityUI[] abilityUI;
    public PlayerAttack playerAttack;

    [Header("Chain UI")]
    public TextMeshProUGUI chargeText;

    private int lastChargeValue = -1;
    private bool uiInitialized = false;

    private Coroutine chargePopRoutine;
    private Coroutine chargeShakeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
            Ability ability = AbilityLoadout.Instance.GetAbility(i);

            if (abilityUI[i] == null) continue;

            if (ability != null)
            {
                abilityUI[i].icon.sprite = ability.icon;
                abilityUI[i].icon.enabled = true;
                abilityUI[i].icon.fillAmount = 0;
            }
            else
            {
                abilityUI[i].icon.enabled = false;
            }
        }
    }

    public void TriggerAbilityUsed(Ability ability)
    {
        // optional hook (kept for compatibility)
    }

    private void UpdateCooldownUI()
    {
        if (AbilityLoadout.Instance == null || playerAttack == null) return;

        for (int i = 0; i < abilityUI.Length; i++)
        {
            Ability ability = AbilityLoadout.Instance.GetAbility(i);
            if (ability == null) continue;

            float remaining = playerAttack.GetCooldownRemaining(ability);

            float fill = (ability.fireRate > 0f)
                ? remaining / ability.fireRate
                : 0f;

            abilityUI[i].icon.fillAmount = Mathf.Clamp01(fill);
        }
    }

    // ---------------- CHAIN UI ----------------

    public void ForceChargeUIRefresh()
    {
        if (chargeText == null) return;

        lastChargeValue = -1;

        chargeText.enabled = chainController.isUnlocked;
        if (chainController.isUnlocked)
            chargeText.text = chainController.hitCounter.ToString();
    }

    private void UpdateChargeUI()
    {
        if (chargeText == null || !uiInitialized || !chainController.isUnlocked)
            return;

        int charges = chainController.hitCounter;
        chargeText.text = charges.ToString();

        if (charges != lastChargeValue)
        {
            if (charges > lastChargeValue)
            {
                if (chargePopRoutine != null) StopCoroutine(chargePopRoutine);
                chargePopRoutine = StartCoroutine(ChargePop());
            }
            else
            {
                if (chargeShakeRoutine != null) StopCoroutine(chargeShakeRoutine);
                chargeShakeRoutine = StartCoroutine(ChargeShake());
            }
        }

        lastChargeValue = charges;
    }

    private IEnumerator ChargePop()
    {
        RectTransform rt = chargeText.rectTransform;

        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.one * Mathf.Lerp(1f, 1.4f, t / 0.15f);
            yield return null;
        }

        t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, t / 0.15f);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    private IEnumerator ChargeShake()
    {
        RectTransform rt = chargeText.rectTransform;
        Vector2 original = rt.anchoredPosition;

        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = original + Random.insideUnitCircle * Mathf.Lerp(4f, 0f, t / 0.12f);
            yield return null;
        }

        rt.anchoredPosition = original;
    }
}