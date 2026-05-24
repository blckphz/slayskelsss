using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float dashSpeed = 12f;
    public Rigidbody2D rb;

    [Header("Stamina Settings")]
    public float maxStamina = 1.5f;
    public float rechargeRate = 0.5f;
    public float consumptionRate = 1.0f;
    public float emptyPenaltyTime = 1f;

    [Header("UI Components")]
    public Slider staminaSlider;
    public Image fillImage;
    public Image backgroundImage;
    public Color normalColor = Color.yellow;
    public Color exhaustedColor = Color.red;

    [Header("Shake Settings")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 5f;

    [Header("Dash Clone Settings")]
    public GameObject dashClonePrefab;
    public float cloneSpawnRate = 0.05f;

    private Vector2 fillOriginalPos;
    private Vector2 bgOriginalPos;

    [HideInInspector]
    public Vector2 moveInput;

    private bool isDashButtonHeld;
    private float currentStamina;
    private bool isExhausted;
    private float cloneTimer;

    void Awake()
    {
        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
        }

        if (fillImage != null)
            fillOriginalPos = fillImage.rectTransform.anchoredPosition;

        if (backgroundImage != null)
            bgOriginalPos = backgroundImage.rectTransform.anchoredPosition;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnDash(InputValue value)
    {
        isDashButtonHeld = value.isPressed;
    }

    void Update()
    {
        if (staminaSlider != null)
            staminaSlider.value = currentStamina;

        if (fillImage != null)
            fillImage.color = isExhausted ? exhaustedColor : normalColor;
    }

    void FixedUpdate()
    {
        HandleStamina();

        bool isMoving = moveInput != Vector2.zero;
        bool canDash = isDashButtonHeld && isMoving && !isExhausted && currentStamina > 0;

        float currentSpeed = canDash ? dashSpeed : moveSpeed;

        rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);

        if (canDash)
        {
            cloneTimer -= Time.fixedDeltaTime;

            if (cloneTimer <= 0f)
            {
                SpawnDashClone();
                cloneTimer = cloneSpawnRate;
            }
        }
        else
        {
            cloneTimer = 0f;
        }
    }

    private void HandleStamina()
    {
        bool isMoving = moveInput != Vector2.zero;

        if (isDashButtonHeld && isMoving && !isExhausted)
        {
            currentStamina -= consumptionRate * Time.fixedDeltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;

                if (!isExhausted)
                {
                    isExhausted = true;
                    Invoke(nameof(ResetExhaustion), emptyPenaltyTime);
                    StartCoroutine(ShakeStaminaUI());
                }
            }
        }
        else
        {
            if (currentStamina < maxStamina)
                currentStamina += rechargeRate * Time.fixedDeltaTime;

            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            if (isExhausted && currentStamina >= maxStamina * 0.2f)
                isExhausted = false;
        }
    }

    // NEW: Check if enough stamina exists
    public bool HasEnoughStamina(float amount)
    {
        return currentStamina >= amount;
    }

    // NEW: Spend stamina
    public bool TryUseStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        return true;
    }

    private void ResetExhaustion()
    {
    }

    private void SpawnDashClone()
    {
        if (dashClonePrefab == null)
            return;

        Instantiate(dashClonePrefab, transform.position, transform.rotation);
    }

    private IEnumerator ShakeStaminaUI()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            Vector2 offset = Random.insideUnitCircle * shakeMagnitude;

            if (fillImage != null)
                fillImage.rectTransform.anchoredPosition = fillOriginalPos + offset;

            if (backgroundImage != null)
                backgroundImage.rectTransform.anchoredPosition = bgOriginalPos + offset;

            yield return null;
        }

        if (fillImage != null)
            fillImage.rectTransform.anchoredPosition = fillOriginalPos;

        if (backgroundImage != null)
            backgroundImage.rectTransform.anchoredPosition = bgOriginalPos;
    }
}