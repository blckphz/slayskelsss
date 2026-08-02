using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNeeds : MonoBehaviour
{
    [Header("Hunger Settings")]
    public int maxSaturation = 100;
    public int saturation = 100;

    public float hungerInterval = 5f;

    [Header("Hunger UI")]
    public Image hungerIcon;
    public Sprite fullChicken;
    public Sprite emptyChicken;
    public TextMeshProUGUI hungerText;

    private float hungerTimer;
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
            hungerTimer = 0f;
        }
    }


    public void AddFood(int amount)
    {
        saturation = Mathf.Clamp(
            saturation + amount,
            0,
            maxSaturation
        );

        UpdateHungerUI();
    }


    private void UpdateHungerUI()
    {
        if (hungerIcon != null)
        {
            if (saturation > 0)
            {
                hungerIcon.sprite = fullChicken;
            }
            else
            {
                hungerIcon.sprite = emptyChicken;
            }
        }


        if (hungerText != null)
        {
            hungerText.text = saturation + "/" + maxSaturation;
        }
    }
}