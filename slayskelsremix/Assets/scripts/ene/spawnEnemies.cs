using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [Header("Spawning Settings")]
    public float spawnInterval = 2.0f; // Spawns every 2 seconds
    public float buffer = 0.1f;        // How far off-screen (0.1 = 10% of screen width)

    private float timer = 0f;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // 1. Increment timer by the time passed since last frame
        timer += Time.deltaTime;

        // 2. Check if it's time to spawn
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f; // Reset the clock
        }
    }

    void SpawnEnemy()
    {
        // Choose a random side: 0=Left, 1=Right, 2=Bottom, 3=Top
        int side = Random.Range(0, 4);
        Vector2 viewportPoint = Vector2.zero;

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

        // Convert viewport (0 to 1) to World Space coordinates
        Vector3 spawnPos = cam.ViewportToWorldPoint(viewportPoint);
        spawnPos.z = 0; // Ensure it's on the 2D plane

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}