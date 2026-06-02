using UnityEngine;

public class chainController : MonoBehaviour
{
    public static int hitCounter;
    public static int staticBonusDmg;
    public static bool isUnlocked;

    [Header("Settings")]
    public int bonusdmg = 5;

    [Header("Debug")]
    [SerializeField] private int currentCharges;

    private void Awake()
    {
        LoadState();

        // 🔥 SAFETY: ensure valid defaults
        if (!PlayerPrefs.HasKey("chain_initialized"))
        {
            isUnlocked = false;
            hitCounter = 0;
            staticBonusDmg = 0;

            SaveState();
            PlayerPrefs.SetInt("chain_initialized", 1);
        }
    }

    private void Update()
    {
        currentCharges = hitCounter;
    }

    private void OnApplicationQuit()
    {
        SaveState();
    }

    private void OnDisable()
    {
        SaveState();
    }

    // 💾 SAVE
    public static void SaveState()
    {
        PlayerPrefs.SetInt("chain_unlocked", isUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("chain_charges", hitCounter);
        PlayerPrefs.SetInt("chain_bonus", staticBonusDmg);
        PlayerPrefs.Save();
    }

    // 📥 LOAD
    public static void LoadState()
    {
        isUnlocked = PlayerPrefs.GetInt("chain_unlocked", 0) == 1;
        hitCounter = PlayerPrefs.GetInt("chain_charges", 0);
        staticBonusDmg = PlayerPrefs.GetInt("chain_bonus", 0);
    }

    // 🔄 RESET
    public static void ResetState()
    {
        PlayerPrefs.DeleteKey("chain_unlocked");
        PlayerPrefs.DeleteKey("chain_charges");
        PlayerPrefs.DeleteKey("chain_bonus");
        PlayerPrefs.DeleteKey("chain_initialized");

        isUnlocked = false;
        hitCounter = 0;
        staticBonusDmg = 0;
    }
}