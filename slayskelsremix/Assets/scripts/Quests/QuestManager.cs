using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Lists")]
    public List<questSO> allQuests = new List<questSO>();
    public List<questSO> activeQuests = new List<questSO>();

    [Header("Settings")]
    public bool autoUnlockNext = true;

    [Header("UI Components")]
    public TextMeshProUGUI questNameText;        // The Title (e.g. "Warmth")
    public TextMeshProUGUI questDescriptionText; // The Desc (e.g. "Build a campfire to stay warm")
    public Image questIcon;

    private string savePath;

    [System.Serializable]
    public class QuestProgressData
    {
        public int questID;
        public int currentAmount;
        public bool isCompleted;
        public bool isActive;
    }

    [System.Serializable]
    public class QuestSaveData
    {
        public List<QuestProgressData> progressList = new List<QuestProgressData>();
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/quests.json";
        LoadQuestProgress();

        if (activeQuests.Count == 0 && allQuests.Count > 0)
        {
            ActivateQuest(allQuests[0].questID);
        }

        UpdateQuestUI();
    }

    public void ActivateQuest(int questID)
    {
        questSO quest = allQuests.Find(q => q.questID == questID);

        if (quest != null && !activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            UpdateQuestUI();
            SaveQuestProgress();
        }
    }

    public void OnBuildingPlaced(buildSO buildingType)
    {
        bool progressMade = false;
        foreach (var quest in activeQuests)
        {
            if (!quest.isCompleted && quest.targetBuilding == buildingType)
            {
                quest.currentAmount++;
                progressMade = true;
                if (quest.currentAmount >= quest.requiredAmount) CompleteQuest(quest);
            }
        }

        if (progressMade)
        {
            UpdateQuestUI();
            SaveQuestProgress();
        }
    }

    private void CompleteQuest(questSO quest)
    {
        quest.isCompleted = true;
        if (autoUnlockNext) UnlockNextQuest(quest);
    }

    private void UnlockNextQuest(questSO currentQuest)
    {
        int currentIndex = allQuests.IndexOf(currentQuest);
        if (currentIndex != -1 && currentIndex + 1 < allQuests.Count)
        {
            ActivateQuest(allQuests[currentIndex + 1].questID);
        }
    }

    // --- UPDATED UI LOGIC ---
    public void UpdateQuestUI()
    {
        questSO displayQuest = activeQuests.Find(q => !q.isCompleted);

        if (displayQuest != null)
        {
            // Set Name
            questNameText.text = displayQuest.questName;

            // Set Description + Progress (e.g. "Build a campfire (0/1)")
            questDescriptionText.text = $"{displayQuest.description} ({displayQuest.currentAmount}/{displayQuest.requiredAmount})";

            if (displayQuest.Icon != null)
            {
                questIcon.sprite = displayQuest.Icon;
                questIcon.enabled = true;
            }
            else
            {
                questIcon.enabled = false;
            }
        }
        else
        {
            questNameText.text = "Finished!";
            questDescriptionText.text = "All tasks complete.";
            questIcon.enabled = false;
        }
    }

    public void SaveQuestProgress()
    {
        QuestSaveData data = new QuestSaveData();
        foreach (var quest in allQuests)
        {
            data.progressList.Add(new QuestProgressData
            {
                questID = quest.questID,
                currentAmount = quest.currentAmount,
                isCompleted = quest.isCompleted,
                isActive = activeQuests.Contains(quest)
            });
        }
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    public void LoadQuestProgress()
    {
        if (!File.Exists(savePath)) return;
        QuestSaveData data = JsonUtility.FromJson<QuestSaveData>(File.ReadAllText(savePath));
        activeQuests.Clear();
        foreach (var pData in data.progressList)
        {
            questSO quest = allQuests.Find(q => q.questID == pData.questID);
            if (quest != null)
            {
                quest.currentAmount = pData.currentAmount;
                quest.isCompleted = pData.isCompleted;
                if (pData.isActive) activeQuests.Add(quest);
            }
        }
    }

    [ContextMenu("Reset All Quest Progress")]
    public void ResetAllQuests()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
        foreach (var quest in allQuests) quest.ResetQuest();
        activeQuests.Clear();
        if (allQuests.Count > 0) ActivateQuest(allQuests[0].questID);
        UpdateQuestUI();
        Debug.Log("Quests Reset!");
    }
}