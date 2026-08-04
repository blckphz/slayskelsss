using UnityEngine;

public class HouseOccuapyScript : MonoBehaviour
{
    [Header("Bed Settings")]
    public Transform sleepPoint;


    public bool IsOccupied { get; private set; }

    public GameObject Owner { get; private set; }



    private void Awake()
    {
        if (sleepPoint == null)
            sleepPoint = transform;
    }



    public bool TryOccupy(GameObject npc)
    {
        if (IsOccupied && Owner != npc)
            return false;


        IsOccupied = true;
        Owner = npc;

        return true;
    }



    public bool CanSleep(GameObject sleeper)
    {
        return !IsOccupied || Owner == sleeper;
    }



    public void Vacate()
    {
        IsOccupied = false;
        Owner = null;
    }



    public Transform GetSleepPoint()
    {
        return sleepPoint;
    }
}