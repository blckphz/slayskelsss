using UnityEngine;
using UnityEngine.UI;

public class PlayerNeeds : MonoBehaviour
{
    [Header("Hunger Settings")]
    public int maxSaturation = 100;
    public int saturation = 100;

    [Tooltip("How many seconds between each hunger loss while moving.")]
    public float hungerInterval = 5f;

    [Header("Hunger UI")]
    public Image[] hungerIcons;      // Assign 5 UI Images
    public Sprite fullChicken;       // Full chicken leg
    public Sprite halfChicken;       // Half chicken leg
    // Optional: public Sprite emptyChicken;

    private float hungerTimer = 0f;
    private PlayerMovement movement;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        UpdateHungerUI();
    }

    void Update()
    {
        if (movement == null)
            return;

        if (movement.IsMoving)
        {
            hungerTimer += Time.deltaTime;

            if (hungerTimer >= hungerInterval)
            {
                hungerTimer = 0f;
                saturation = Mathf.Max(0, saturation - 1);
                UpdateHungerUI();
            }
        }
        else
        {
            // Reset timer when the player stops moving
            hungerTimer = 0f;
        }
    }

    public void AddFood(int amount)
    {
        saturation = Mathf.Clamp(saturation + amount, 0, maxSaturation);
        UpdateHungerUI();
    }

    private void UpdateHungerUI()
    {
        // Each icon represents 20 saturation
        int remaining = saturation;

        for (int i = 0; i < hungerIcons.Length; i++)
        {
            if (remaining >= 20)
            {
                hungerIcons[i].enabled = true;
                hungerIcons[i].sprite = fullChicken;
            }
            else if (remaining >= 10)
            {
                hungerIcons[i].enabled = true;
                hungerIcons[i].sprite = halfChicken;
            }
            else
            {
                // Hide the icon completely when empty
                hungerIcons[i].enabled = false;

                // If you have an empty sprite instead, use:
                // hungerIcons[i].enabled = true;
                // hungerIcons[i].sprite = emptyChicken;
            }

            remaining -= 20;
        }
    }
}