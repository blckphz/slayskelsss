using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [Header("Spawning Settings")]
    public float spawnInterval;
    public float buffer = 0.1f;

    [Header("References")]
    public DayNightCycle dayNightCycle;

    private float timer = 0f;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (dayNightCycle == null)
        {
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
        }
    }

    void Update()
    {
        // 1 = "none", so stop spawning
        if (PlayerPrefs.GetInt("Enemies", 0) == 1)
            return;

        // Only spawn at night (6 PM - 6 AM)
        if (dayNightCycle == null || !IsNight())
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    bool IsNight()
    {
        float hour = dayNightCycle.RawTime * 24f;
        return hour >= 18f || hour < 6f;
    }

    void SpawnEnemy()
    {
        int side = Random.Range(0, 4);
        Vector3 viewportPoint = Vector3.zero;

        switch (side)
        {
            case 0: // Left
                viewportPoint = new Vector3(-buffer, Random.value, 10f);
                break;

            case 1: // Right
                viewportPoint = new Vector3(1f + buffer, Random.value, 10f);
                break;

            case 2: // Bottom
                viewportPoint = new Vector3(Random.value, -buffer, 10f);
                break;

            case 3: // Top
                viewportPoint = new Vector3(Random.value, 1f + buffer, 10f);
                break;
        }

        Vector3 spawnPos = cam.ViewportToWorldPoint(viewportPoint);
        spawnPos.z = 0f;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}