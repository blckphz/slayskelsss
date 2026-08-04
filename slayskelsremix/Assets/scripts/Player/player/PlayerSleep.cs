using System.Collections;
using UnityEngine;

public class PlayerSleep : MonoBehaviour
{
    private HouseOccuapyScript bed;
    private DayNightCycle dayNight;

    private Transform player;
    private Transform wakePoint;

    private bool playerInside;
    private bool sleeping;

    private void Awake()
    {
        bed = GetComponent<HouseOccuapyScript>();
        dayNight = FindFirstObjectByType<DayNightCycle>();
        wakePoint = transform;

        Debug.Log("[PlayerSleep] Initialized");
    }

    private void Update()
    {
        if (sleeping)
            return;

        if (!playerInside)
            return;

        if (dayNight == null)
            return;

        // Only show prompt at night
        if (!dayNight.IsNight())
        {
            if (InteractionUI.Instance != null)
                InteractionUI.Instance.Hide();

            return;
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Show("Press Z to Sleep");
        }

        if (PlayerInputHandler.Instance != null && PlayerInputHandler.Instance.SleepPressed())
        {
            Debug.Log("[PlayerSleep] Sleep pressed");
            TrySleep();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        player = other.transform;
        playerInside = true;

        Debug.Log("[PlayerSleep] Player entered bed");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        player = null;

        Debug.Log("[PlayerSleep] Player left bed");

        if (InteractionUI.Instance != null)
            InteractionUI.Instance.Hide();
    }

    private void TrySleep()
    {
        if (!dayNight.IsNight())
        {
            Debug.Log("[PlayerSleep] Cannot sleep during day");
            return;
        }

        if (player == null)
            return;

        if (!bed.CanSleep(player.gameObject))
        {
            Debug.Log("[PlayerSleep] Bed occupied");
            return;
        }

        sleeping = true;

        Debug.Log("[PlayerSleep] Going to sleep");

        player.position = bed.GetSleepPoint().position;

        dayNight.StartSleeping();

        if (InteractionUI.Instance != null)
            InteractionUI.Instance.Hide();

        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        Debug.Log("[PlayerSleep] Waiting for morning");

        // Wait until time progresses into the 6:00 AM - 12:00 PM morning window
        while (!dayNight.IsMorning())
        {
            yield return null;
        }

        Debug.Log("[PlayerSleep] Morning - waking up");

        dayNight.StopSleeping();

        if (player != null)
        {
            player.position = wakePoint.position;
        }

        sleeping = false;
    }
}