using UnityEngine;
using System.Collections;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform spawnPoint;
    public HitZone hitZone;
    public fishingManager fishingManager;

    public float endDelay = 1.5f;
    private Coroutine endRoutine;
    public bool IsSpawningFinished;

    public void SpawnNote()
    {
        GameObject noteObj = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity, transform);
        NoteController controller = noteObj.GetComponent<NoteController>();

        if (controller != null && hitZone != null)
        {
            controller.Init(hitZone);
        }
    }

    public void FinishSpawning()
    {
        IsSpawningFinished = true;
        Debug.Log("<color=yellow>[Spawner]</color> Spawning flagged as Finished. Starting Timeout Routine.");
        if (endRoutine != null) StopCoroutine(endRoutine);
        endRoutine = StartCoroutine(EndAfterDelay());
    }

    private IEnumerator EndAfterDelay()
    {
        yield return new WaitForSeconds(endDelay);
        if (fishingManager != null && IsSpawningFinished)
        {
            Debug.Log("<color=red>[Spawner]</color> Safety Timeout Reached! Forcing End.");
            fishingManager.EndFishing(true, fishingManager.EndReason.Timeout);
        }
    }

    public void CancelEndRoutine()
    {
        if (endRoutine != null)
        {
            Debug.Log("<color=green>[Spawner]</color> Safety Timeout Cancelled (Game ended normally).");
            StopCoroutine(endRoutine);
            endRoutine = null;
        }
    }

    public void ResetSpawner()
    {
        CancelEndRoutine();
        IsSpawningFinished = false;
        foreach (Transform child in transform) Destroy(child.gameObject);
    }
}