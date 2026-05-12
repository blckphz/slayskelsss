using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int currentXP;
    public int level = 1;
    public int xpToNextLevel = 100;

    public void AddXP(int amount)
    {
        currentXP += amount;

        Debug.Log("XP gained: " + amount);

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;

        xpToNextLevel += 50;

        Debug.Log("LEVEL UP! Level: " + level);
    }
}