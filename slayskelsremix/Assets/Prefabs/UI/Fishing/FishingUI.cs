using UnityEngine;
using System.Collections.Generic;

public class FishingUI : MonoBehaviour
{
    public bool gameActive;

    public NoteSpawner spawner;

    public List<float> noteTimes = new List<float> { 3f, 6f, 9f };

    private float timer;
    private int index;

    public void StartGame()
    {
        gameActive = true;
        timer = 0;
        index = 0;
    }

    public void StopGame()
    {
        gameActive = false;
    }

    private void Update()
    {
        if (!gameActive) return;

        timer += Time.deltaTime;

        if (index < noteTimes.Count && timer >= noteTimes[index])
        {
            spawner.SpawnNote();
            index++;
        }
    }
}