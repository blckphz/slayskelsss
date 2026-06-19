using UnityEngine;
using TMPro;

public class difficultyManager : MonoBehaviour
{
    public TMP_Dropdown enemyDropdown;

    void Start()
    {
        enemyDropdown.onValueChanged.AddListener(OnEnemySettingChanged);

        // load saved value (default = "some" → 0)
        enemyDropdown.value = PlayerPrefs.GetInt("Enemies", 0);
        OnEnemySettingChanged(enemyDropdown.value);
    }

    void OnEnemySettingChanged(int index)
    {
        // 0 = "some", 1 = "none"
        PlayerPrefs.SetInt("Enemies", index);
        PlayerPrefs.Save();
    }
}