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

    public void SaveEnemies()
    {
        enemyHealth[] enemies = FindObjectsOfType<enemyHealth>();
        EnemySaveList saveList = new EnemySaveList();

        SceneLoader.Instance?.SetLoadingText("Saving enemies...");

        foreach (var e in enemies)
        {
            SceneLoader.Instance?.SetLoadingText(
                $"Saving enemy {e.enemyID}..."
            );

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

        SceneLoader.Instance?.SetLoadingText(
            "Enemies saved successfully."
        );
    }

    public void LoadEnemies()
    {
        SceneLoader.Instance?.SetLoadingText(
            "Loading enemies..."
        );

        if (!File.Exists(path))
        {
            SceneLoader.Instance?.SetLoadingText(
                "No enemy save found."
            );
            return;
        }

        string json = File.ReadAllText(path);
        EnemySaveList saveList =
            JsonUtility.FromJson<EnemySaveList>(json);

        enemyHealth[] enemies =
            FindObjectsOfType<enemyHealth>();

        foreach (var savedEnemy in saveList.enemies)
        {
            SceneLoader.Instance?.SetLoadingText(
                $"Restoring enemy {savedEnemy.id}..."
            );

            foreach (var sceneEnemy in enemies)
            {
                if (sceneEnemy.enemyID == savedEnemy.id)
                {
                    sceneEnemy.transform.position =
                        new Vector2(
                            savedEnemy.x,
                            savedEnemy.y
                        );

                    sceneEnemy.currentHealth =
                        savedEnemy.health;

                    sceneEnemy.UpdateHealthUI();
                    break;
                }
            }
        }

        SceneLoader.Instance?.SetLoadingText(
            "Enemies loaded successfully."
        );
    }
}