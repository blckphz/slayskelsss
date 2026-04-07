using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HitZone : MonoBehaviour
{
    public bool gameActive;
    public List<NoteController> activeNotes = new List<NoteController>();

    public float perfectDistance;
    public float goodDistance;

    public InputActionReference hitAction;

    public NoteSpawner spawner;
    public fishingManager fishingManager;

    private void Start()
    {
        StartGame();
    }

    private void OnEnable()
    {
        if (hitAction == null) return;

        hitAction.action.performed += OnHit;
        hitAction.action.Enable();
    }

    private void OnDisable()
    {
        if (hitAction == null) return;

        hitAction.action.performed -= OnHit;
        hitAction.action.Disable();
    }

    public void StartGame()
    {
        gameActive = true;
        activeNotes.Clear();
    }

    public void RegisterNote(NoteController note)
    {
        activeNotes.Add(note);
    }

    public void UnregisterNote(NoteController note)
    {
        activeNotes.Remove(note);
        CheckAutoEnd();
    }

    private void OnHit(InputAction.CallbackContext context)
    {
        TryHit();
    }

    public void TryHit()
    {
        if (!gameActive) return;

        activeNotes.RemoveAll(n => n == null);

        if (activeNotes.Count == 0) return;

        NoteController best = null;
        float bestDist = Mathf.Infinity;

        foreach (var note in activeNotes)
        {
            float dist = Vector2.Distance(transform.position, note.transform.position);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = note;
            }
        }

        if (best == null) return;

        if (bestDist <= perfectDistance)
            Debug.Log("🔥 PERFECT");
        else if (bestDist <= goodDistance)
            Debug.Log("👍 GOOD");
        else
            Debug.Log("👎 BAD");

        best.OnHit();

        CheckAutoEnd();
    }

    private void CheckAutoEnd()
    {
        activeNotes.RemoveAll(n => n == null);

        if (!gameActive) return;
        if (spawner == null) return;

        if (spawner.IsSpawningFinished && activeNotes.Count == 0)
        {
            Debug.Log("🏁 ALL NOTES CLEARED → END");

            gameActive = false;

            spawner.CancelEndRoutine();

            if (fishingManager != null)
                fishingManager.EndFishing(true, fishingManager.EndReason.Completed);
        }
    }
}