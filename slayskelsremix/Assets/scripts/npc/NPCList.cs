using UnityEngine;
using System.Collections.Generic;

public class NPCList : MonoBehaviour
{
    public static NPCList Instance;

    // Key: Target ID, Value: NPC ID
    private Dictionary<int, int> claimedTargets = new Dictionary<int, int>();

    // IDs that were JUST destroyed and should be ignored by everyone
    private HashSet<int> pendingDestruction = new HashSet<int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void MarkAsDestroyed(int id)
    {
        if (!pendingDestruction.Contains(id))
        {
            pendingDestruction.Add(id);
            // Clear from blacklist after 0.1s once Unity has fully removed the object
            Invoke(nameof(ClearPending), 0.1f);
        }
    }

    private void ClearPending() => pendingDestruction.Clear();

    public bool IsTargetClaimed(Transform target, int requesterID)
    {
        if (target == null) return true;

        int targetID = target.GetInstanceID();

        // 1. Is it currently being destroyed?
        if (pendingDestruction.Contains(targetID)) return true;

        // 2. Is it a chest? (Multiple NPCs can use one chest)
        if (target.GetComponent<ChestInventory>() != null) return false;

        // 3. Is someone else already heading there?
        if (claimedTargets.ContainsKey(targetID))
        {
            if (claimedTargets[targetID] != requesterID) return true;
        }

        return false;
    }

    public void ClaimTarget(Transform target, int requesterID)
    {
        if (target == null || target.GetComponent<ChestInventory>() != null) return;
        claimedTargets[target.GetInstanceID()] = requesterID;
    }

    public void ReleaseTarget(int requesterID)
    {
        int keyToRemove = -1;
        foreach (var pair in claimedTargets)
        {
            if (pair.Value == requesterID) { keyToRemove = pair.Key; break; }
        }
        if (keyToRemove != -1) claimedTargets.Remove(keyToRemove);
    }
}