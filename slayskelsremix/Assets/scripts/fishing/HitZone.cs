using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HitZone : MonoBehaviour
{
    public bool gameActive;
    public List<NoteController> activeNotes = new List<NoteController>();

    public float perfectDistance = 0.5f;
    public float goodDistance = 1.0f;

    public InputActionReference hitAction;
    public NoteSpawner spawner;
    public fishingManager fishingManager;

    private void OnEnable() { BindInput(); }
    private void OnDisable() { UnbindInput(); }

    private void BindInput()
    {
        if (hitAction == null) return;
        hitAction.action.performed -= OnHit; // Prevent double binding
        hitAction.action.performed += OnHit;
        hitAction.action.Enable();
    }

    private void UnbindInput()
    {
        if (hitAction == null) return;
        hitAction.action.performed -= OnHit;
    }

    public void StartGame()
    {
        gameActive = true;
        activeNotes.Clear();
        Debug.Log("<color=blue>[HitZone]</color> Initialized and list cleared.");
    }

    public void RegisterNote(NoteController note)
    {
        if (!activeNotes.Contains(note)) activeNotes.Add(note);
        Debug.Log($"<color=blue>[HitZone]</color> Note Registered. Count: {activeNotes.Count}");
    }

    public void UnregisterNote(NoteController note)
    {
        activeNotes.Remove(note);
        Debug.Log($"<color=blue>[HitZone]</color> Note Unregistered. Count: {activeNotes.Count}");
        CheckAutoEnd();
    }

    private void OnHit(InputAction.CallbackContext context) { TryHit(); }

    public void TryHit()
    {
        if (!gameActive) return;

        activeNotes.RemoveAll(n => n == null);
        Debug.Log($"<color=white>[Input]</color> Space Pressed. Notes in zone: {activeNotes.Count}");

        if (activeNotes.Count == 0) return;

        NoteController best = null;
        float bestDist = Mathf.Infinity;

        foreach (var note in activeNotes)
        {
            float dist = Vector2.Distance(transform.position, note.transform.position);
            if (dist < bestDist) { bestDist = dist; best = note; }
        }

        if (best != null)
        {
            if (bestDist <= perfectDistance) Debug.Log("<color=green>🔥 PERFECT</color> (Dist: " + bestDist.ToString("F2") + ")");
            else if (bestDist <= goodDistance) Debug.Log("<color=yellow>👍 GOOD</color> (Dist: " + bestDist.ToString("F2") + ")");
            else Debug.Log("<color=red>👎 BAD</color> (Dist: " + bestDist.ToString("F2") + ")");

            best.OnHit();
        }
    }

    private void CheckAutoEnd()
    {
        if (spawner != null && spawner.IsSpawningFinished && activeNotes.Count == 0)
        {
            if (gameActive)
            {
                gameActive = false;
                Debug.Log("<color=magenta>🏁 [HitZone]</color> All notes cleared. Ending Fishing Session.");
                spawner.CancelEndRoutine();
                if (fishingManager != null)
                    fishingManager.EndFishing(true, fishingManager.EndReason.Completed);
            }
        }
    }
}