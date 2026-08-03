using UnityEngine;

public class HouseOccuapyScript : MonoBehaviour
{
    [Header("House Settings")]
    public Transform sleepPoint; // Transform where NPC stands/sleeps inside the house

    public bool IsOccupied { get; private set; }
    public GameObject Owner { get; private set; }

    private void Awake()
    {
        // If no specific sleep point is assigned, use this object's transform
        if (sleepPoint == null)
            sleepPoint = transform;
    }

    /// <summary>
    /// Attempts to claim the house for an NPC.
    /// </summary>
    public bool TryOccupy(GameObject npc)
    {
        if (IsOccupied && Owner != npc)
            return false;

        IsOccupied = true;
        Owner = npc;
        return true;
    }

    /// <summary>
    /// Vacates the house.
    /// </summary>
    public void Vacate()
    {
        IsOccupied = false;
        Owner = null;
    }
}