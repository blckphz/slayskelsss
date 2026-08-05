using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Dash Settings")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.3f;
    public float dashStaminaCost = 0.4f;

    [Header("Stamina Settings")]
    public float maxStamina = 1.5f;
    public float rechargeRate = 0.5f;

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

    [Header("Dash Effects")]
    public ParticleSystem dashParticles;

    private Vector2 fillOriginalPos;
    private Vector2 bgOriginalPos;

    [HideInInspector]
    public Vector2 moveInput;

    private float currentStamina;
    private bool isExhausted;

    private bool isDashing;
    private bool canDash = true;
    private Vector2 dashDirection;
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

        if (dashParticles != null)
            dashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnDash(InputValue value)
    {
        if (value.isPressed)
            TryDash();
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

        float speed = isDashing ? dashSpeed : moveSpeed;
        Vector2 direction = isDashing ? dashDirection : moveInput;

        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (isDashing)
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

    private void TryDash()
    {
        if (!canDash)
            return;

        if (isDashing)
            return;

        if (moveInput == Vector2.zero)
            return;

        if (!TryUseStamina(dashStaminaCost))
        {
            if (!isExhausted)
            {
                isExhausted = true;
                StartCoroutine(ShakeStaminaUI());
            }
            return;
        }

        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;

        // Small camera shake when dash starts
        CameraShaker.Instance?.Shake(0.8f, 0.08f);

        dashDirection = moveInput.normalized;

        // Start dash particles
        if (dashParticles != null)
        {
            dashParticles.Clear();
            dashParticles.Play();
        }

        float timer = dashDuration;

        while (timer > 0f)
        {
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        // Stop emitting new particles but let existing ones fade naturally
        if (dashParticles != null)
            dashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void HandleStamina()
    {
        if (!isDashing && currentStamina < maxStamina)
        {
            currentStamina += rechargeRate * Time.fixedDeltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }

        if (isExhausted && currentStamina >= dashStaminaCost)
            isExhausted = false;
    }

    public bool HasEnoughStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public bool TryUseStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        return true;
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

    public bool IsMoving => moveInput != Vector2.zero;
}