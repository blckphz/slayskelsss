using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EnemySaveSystem : MonoBehaviour
{
    public static EnemySaveSystem Instance;

    private string path;

    private void Awake()
    {
        Instance = this;
        path = Application.persistentDataPath + "/enemies.json";
    }

    // =========================
    // SAVE
    // =========================
    public void SaveEnemies()
    {
        enemyHealth[] enemies = FindObjectsOfType<enemyHealth>();

        EnemySaveList saveList = new EnemySaveList();

        foreach (var e in enemies)
        {
            EnemySaveData data = new EnemySaveData
            {
                id = e.enemyID,
                x = e.transform.position.x,
                y = e.transform.position.y,
                health = e.currentHealth
            };

            saveList.enemies.Add(data);
        }

        string json = JsonUtility.ToJson(saveList, true);
        File.WriteAllText(path, json);

        Debug.Log("Enemies saved to: " + path);
    }

    // =========================
    // LOAD
    // =========================
    public void LoadEnemies()
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        string json = File.ReadAllText(path);
        EnemySaveList saveList = JsonUtility.FromJson<EnemySaveList>(json);

        enemyHealth[] enemies = FindObjectsOfType<enemyHealth>();

        foreach (var savedEnemy in saveList.enemies)
        {
            foreach (var sceneEnemy in enemies)
            {
                if (sceneEnemy.enemyID == savedEnemy.id)
                {
                    sceneEnemy.transform.position = new Vector2(savedEnemy.x, savedEnemy.y);
                    sceneEnemy.currentHealth = savedEnemy.health;
                    sceneEnemy.UpdateHealthUI();
                    break;
                }
            }
        }

        Debug.Log("Enemies loaded.");
    }
}