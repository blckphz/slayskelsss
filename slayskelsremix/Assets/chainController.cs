using UnityEngine;

public class chainController : MonoBehaviour
{
    public static int hitCcunter; // The "Battery"
    public static float staticBonusDmg;

    [Header("Settings")]
    public float bonusdmg = 5f; // Extra damage per charge

    [Header("Debug (View Only)")]
    [SerializeField] private int currentCharges;

    void Awake()
    {
        staticBonusDmg = bonusdmg;
        hitCcunter = 0;
    }

    void Update()
    {
        // Makes the static value visible in Inspector for debugging
        currentCharges = hitCcunter;
    }
}