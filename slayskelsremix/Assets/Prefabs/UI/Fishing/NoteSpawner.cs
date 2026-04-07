using UnityEngine;
using System.Collections;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform spawnPoint;
    public Transform container;

    public HitZone hitZone;
    public fishingManager fishingManager;

    public int totalNotesToSpawn = 10;
    public float endDelay = 3.5f;

    private int spawnedNotes;
    private Coroutine endRoutine;

    public bool IsSpawningFinished => spawnedNotes >= totalNotesToSpawn;

    public void SpawnNote()
    {
        GameObject obj = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity, container);

        NoteController note = obj.GetComponent<NoteController>();

        if (note != null)
            note.Init(hitZone);

        spawnedNotes++;

        Debug.Log($"🎵 Spawned {spawnedNotes}/{totalNotesToSpawn}");

        if (spawnedNotes >= totalNotesToSpawn)
        {
            Debug.Log("🏁 LAST NOTE SPAWNED");

            if (endRoutine == null)
                endRoutine = StartCoroutine(EndAfterDelay());
        }
    }

    private IEnumerator EndAfterDelay()
    {
        yield return new WaitForSeconds(endDelay);

        if (endRoutine == null) yield break;

        Debug.Log("🏁 END (timeout)");

        if (fishingManager != null)
            fishingManager.EndFishing(true);

        endRoutine = null;
    }

    public void CancelEndRoutine()
    {
        if (endRoutine != null)
        {
            StopCoroutine(endRoutine);
            endRoutine = null;
            Debug.Log("🛑 End routine cancelled");
        }
    }

    public void ResetSpawner()
    {
        spawnedNotes = 0;
        CancelEndRoutine();
    }
}