using UnityEngine;

public class PlayerNeeds : MonoBehaviour
{
    [Header("Hunger Settings")]
    public int maxSaturation = 100;
    public int saturation = 100;

    public float hungerPerSecondWhileMoving = 1f;

    private PlayerMovement movement;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (movement == null) return;

        bool isMoving = movement.IsMoving;

        if (isMoving)
        {
            saturation -= Mathf.CeilToInt(hungerPerSecondWhileMoving * Time.deltaTime);
            saturation = Mathf.Clamp(saturation, 0, maxSaturation);
        }
    }

    public void AddFood(int amount)
    {
        saturation = Mathf.Clamp(saturation + amount, 0, maxSaturation);
    }
}