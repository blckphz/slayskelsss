using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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

    [Header("Chain UI")]
    public TextMeshProUGUI chargeText;

    private int lastChargeValue = -1;
    private Coroutine chargePopRoutine;
    private Coroutine chargeShakeRoutine;

    private bool uiInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[UI] charsetter Awake");
    }

    void Start()
    {
        UpdateAbilityIcons();

        ForceChargeUIRefresh();

        uiInitialized = true;

        Debug.Log("[UI] charsetter Start COMPLETE");
    }

    void Update()
    {
        UpdateCooldownUI();
        UpdateChargeUI();
    }

    public void UpdateAbilityIcons()
    {
        if (selectedChar == null || selectedChar.abilities == null)
        {
            Debug.LogWarning("[UI] No character abilities assigned");
            return;
        }

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

        Debug.Log("[UI] Ability icons updated");
    }

    public void TriggerAbilityUsed(Ability ability)
    {
        if (ability == null) return;

        abilityUsedTime[ability] = Time.time;

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

            float fill = remaining > 0f ? remaining / ability.fireRate : 0f;
            abilityUI[i].fillImage.fillAmount = fill;
        }
    }

    // 🔥 CHARGE UI
    private void UpdateChargeUI()
    {
        if (chargeText == null)
            return;

        if (!uiInitialized)
            return;

        if (!chainController.isUnlocked)
        {
            if (chargeText.enabled)
            {
                Debug.Log("[UI] Charge UI disabled (not unlocked)");
            }

            chargeText.enabled = false;
            chargeText.text = "";
            lastChargeValue = -1;
            return;
        }

        if (!chargeText.enabled)
        {
            chargeText.enabled = true;
            Debug.Log("[UI] Charge UI re-enabled");
        }

        int charges = chainController.hitCounter;
        chargeText.text = charges.ToString();

        if (charges > lastChargeValue)
        {
            Debug.Log($"[UI] Charge INCREASE: {lastChargeValue} → {charges}");

            if (chargePopRoutine != null)
                StopCoroutine(chargePopRoutine);

            chargePopRoutine = StartCoroutine(ChargePop());
        }
        else if (charges < lastChargeValue)
        {
            Debug.Log($"[UI] Charge DECREASE: {lastChargeValue} → {charges}");

            if (chargeShakeRoutine != null)
                StopCoroutine(chargeShakeRoutine);

            chargeShakeRoutine = StartCoroutine(ChargeShake());
        }

        lastChargeValue = charges;

        // color feedback
        if (charges <= 0)
            chargeText.color = Color.gray;
        else if (charges < 5)
            chargeText.color = Color.white;
        else
            chargeText.color = Color.yellow;
    }

    // 🔥 FIXED FORCE REFRESH
    public void ForceChargeUIRefresh()
    {
        Debug.Log("[UI] ForceChargeUIRefresh CALLED");

        if (chargeText == null)
        {
            Debug.LogWarning("[UI] chargeText is NULL");
            return;
        }

        lastChargeValue = -1;

        if (chainController.isUnlocked)
        {
            chargeText.enabled = true;
            chargeText.text = chainController.hitCounter.ToString();

            Debug.Log($"[UI] Forced display: {chainController.hitCounter}");
        }
        else
        {
            chargeText.enabled = false;
        }
    }

    // ⚡ POP
    private IEnumerator ChargePop()
    {
        RectTransform rt = chargeText.rectTransform;

        rt.localScale = Vector3.one;

        float duration = 0.15f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(1f, 1.4f, t / duration);
            rt.localScale = Vector3.one * s;
            yield return null;
        }

        t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(1.4f, 1f, t / duration);
            rt.localScale = Vector3.one * s;
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    // 💥 SHAKE
    private IEnumerator ChargeShake()
    {
        RectTransform rt = chargeText.rectTransform;
        Vector2 originalPos = rt.anchoredPosition;

        float time = 0f;

        while (time < 0.12f)
        {
            time += Time.deltaTime;

            float strength = Mathf.Lerp(4f, 0f, time / 0.12f);
            Vector2 offset = Random.insideUnitCircle * strength;

            rt.anchoredPosition = originalPos + offset;

            yield return null;
        }

        rt.anchoredPosition = originalPos;
    }
}