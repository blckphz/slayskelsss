using UnityEngine;
using System.IO;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelData
    {
        public int level;
        public int currentXP;
        public int xpToNextLevel;
        public int skillPoints;
    }

    [Header("Progression")]
    public int level = 1;
    public int currentXP;
    public int xpToNextLevel = 100;
    public int skillPoints = 0;

    [Header("Save Settings")]
    public string saveID;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveID + "_level.json");

    void Awake()
    {
        // For Player: Set saveID to "Player" in Inspector
        // For NPCs: enemyHealth script will assign its unique ID here
        if (string.IsNullOrEmpty(saveID))
        {
            saveID = gameObject.name + "_" + transform.GetInstanceID();
        }
    }

    void Start()
    {
        LoadLevelData();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        SaveLevelData();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        xpToNextLevel += 50;
        skillPoints++;

        Debug.Log($"<color=yellow>{gameObject.name} LEVELED UP! Level: {level} | Points: {skillPoints}</color>");
    }

    public void SpendSkillPoint(int amount)
    {
        if (skillPoints >= amount)
        {
            skillPoints -= amount;
            SaveLevelData();
        }
    }

    public void SaveLevelData()
    {
        LevelData data = new LevelData
        {
            level = this.level,
            currentXP = this.currentXP,
            xpToNextLevel = this.xpToNextLevel,
            skillPoints = this.skillPoints
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public void LoadLevelData()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            this.level = data.level;
            this.currentXP = data.currentXP;
            this.xpToNextLevel = data.xpToNextLevel;
            this.skillPoints = data.skillPoints;

            Debug.Log($"{gameObject.name} Loaded: Level {level}");
        }
    }
}