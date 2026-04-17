using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FishingUI : MonoBehaviour
{
    public bool gameActive;
    public NoteSpawner spawner;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip bubbleSound;
    [Range(0.0f, 0.5f)] public float pitchRange = 0.15f;

    [Header("Timing List")]
    public List<float> noteTimes = new List<float> { 0.5f, 1.0f, 1.5f, 2.0f };

    [Header("Sequence Settings")]
    [Tooltip("Delay after the second-to-last bubble before notes start spawning. 0.1-0.2 is usually 'tighter'.")]
    public float overlapDelay = 0.2f;

    private float timer;
    private int currentIndex;
    private bool isSpawningPhase;
    private bool finishedSpawningCalled;
    private bool transitionTriggered;

    public void StartGame()
    {
        StopAllCoroutines();
        gameActive = true;
        timer = 0f;
        currentIndex = 0;
        isSpawningPhase = false;
        finishedSpawningCalled = false;
        transitionTriggered = false;
        Debug.Log("<color=cyan><b>[FishingUI]</b></color> 🎣 Game Started. Phase 1: Audio Cues.");
    }

    public void StopGame()
    {
        gameActive = false;
        StopAllCoroutines();
        Debug.Log("<color=red><b>[FishingUI]</b></color> 🛑 Game Logic Stopped.");
    }

    private void Update()
    {
        if (!gameActive) return;

        timer += Time.deltaTime;

        // Check if it's time for the next note/sound in the list
        if (currentIndex < noteTimes.Count && timer >= noteTimes[currentIndex])
        {
            if (!isSpawningPhase)
            {
                Debug.Log($"<color=white>[Audio]</color> Bubble {currentIndex + 1}/{noteTimes.Count} triggered at {timer:F2}s");
                PlayBubble();

                // 🔥 THE TIGHT TRANSITION
                // Trigger transition when we play the second-to-last bubble
                if (currentIndex == noteTimes.Count - 1 && !transitionTriggered)
                {
                    transitionTriggered = true;
                    StartCoroutine(QuickTransition());
                }
            }
            else
            {
                Debug.Log($"<color=yellow>[Spawn]</color> Spawning Note {currentIndex + 1}/{noteTimes.Count} at {timer:F2}s");
                SpawnNote();
            }

            currentIndex++;
        }

        // Finalize state when all notes in the list have been spawned
        if (isSpawningPhase && currentIndex >= noteTimes.Count && !finishedSpawningCalled)
        {
            finishedSpawningCalled = true;
            Debug.Log("<color=green>[UI]</color> All notes spawned. Signaling Spawner to finish.");
            if (spawner != null) spawner.FinishSpawning();
        }
    }

    private void PlayBubble()
    {
        if (audioSource != null && bubbleSound != null)
        {
            audioSource.pitch = 1f + Random.Range(-pitchRange, pitchRange);
            audioSource.PlayOneShot(bubbleSound);
        }
    }

    private void SpawnNote()
    {
        if (spawner != null) spawner.SpawnNote();
    }

    private IEnumerator QuickTransition()
    {
        yield return new WaitForSeconds(overlapDelay);

        if (!gameActive) yield break;

        // Reset timer and index for Phase 2
        timer = 0f;
        currentIndex = 0;
        isSpawningPhase = true;

        Debug.Log("<color=orange><b>[TRANSITION]</b></color> ⚡ Timer reset! Phase 2: Spawning Phase ACTIVE.");
    }
}