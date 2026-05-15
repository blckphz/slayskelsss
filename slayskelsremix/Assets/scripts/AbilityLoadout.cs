using UnityEngine;
using System.Collections;

public class AbilityLoadout : MonoBehaviour
{
    public static AbilityLoadout Instance { get; private set; }

    public Ability[] equippedAbilities = new Ability[4];

    [Header("Settings")]
    public AbilityDatabase abilityDatabase;
    private const string SAVE_KEY_PREFIX = "AbilitySlot_";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        LoadLoadout();
        // Delay visual sync by one frame to ensure UI is initialized
        StartCoroutine(InitialSyncRoutine());
    }

    private IEnumerator InitialSyncRoutine()
    {
        yield return new WaitForEndOfFrame();
        RefreshAllUI();
    }

    public void SetAbility(int index, Ability ability)
    {
        if (index < 0 || index >= equippedAbilities.Length) return;

        equippedAbilities[index] = ability;
        SaveLoadout();
        RefreshAllUI();
    }

    public Ability GetAbility(int index)
    {
        if (index < 0 || index >= equippedAbilities.Length) return null;
        return equippedAbilities[index];
    }

    public void SaveLoadout()
    {
        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            if (equippedAbilities[i] != null)
                PlayerPrefs.SetString(SAVE_KEY_PREFIX + i, equippedAbilities[i].abilityName);
            else
                PlayerPrefs.SetString(SAVE_KEY_PREFIX + i, "Empty");
        }
        PlayerPrefs.Save();
    }

    public void LoadLoadout()
    {
        if (abilityDatabase == null) return;

        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            string savedName = PlayerPrefs.GetString(SAVE_KEY_PREFIX + i, "Empty");
            equippedAbilities[i] = (savedName == "Empty") ? null : abilityDatabase.GetAbilityByName(savedName);
        }
    }

    public void RefreshAllUI()
    {
        // Update the hotbar icons (charsetter)
        if (charsetter.Instance != null)
            charsetter.Instance.UpdateAbilityIcons();

        // Update the drag-and-drop slots (AbilityDropSlot)
        AbilityDropSlot[] slots = FindObjectsOfType<AbilityDropSlot>();
        foreach (var slot in slots)
        {
            slot.UpdateSlotUI(GetAbility(slot.slotIndex));
        }
    }
}