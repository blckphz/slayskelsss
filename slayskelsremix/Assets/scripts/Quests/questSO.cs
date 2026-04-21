using UnityEngine;

[CreateAssetMenu(fileName = "New Build Quest", menuName = "Quests/Build Quest")]
public class questSO : ScriptableObject
{
    public int questID; // Unique ID for saving (e.g., 1, 2, 3...)
    public string questName;
    [TextArea] public string description;

    [Header("Requirements")]
    public buildSO targetBuilding; // Drag your buildable item here
    public int requiredAmount;

    // Automatically pulls the icon from the item's SO
    public Sprite Icon => targetBuilding != null ? targetBuilding.icon : null;

    [Header("Current Progress")]
    public int currentAmount;
    public bool isCompleted;

    public void ResetQuest()
    {
        currentAmount = 0;
        isCompleted = false;
    }
}