using UnityEngine;

public static class ActionLock
{
    public static bool IsLocked { get; private set; }

    // NEW: tells other systems “this click already belongs to something”
    public static bool ConsumeInputThisFrame { get; private set; }

    private static int lockFrame = -1;

    public static void LockThisFrame()
    {
        IsLocked = true;
        ConsumeInputThisFrame = true;
        lockFrame = Time.frameCount;

        Debug.Log($"[ActionLock] LOCK ACTIVATED on frame {lockFrame}");
    }

    public static void Tick()
    {
        if (IsLocked && Time.frameCount != lockFrame)
        {
            IsLocked = false;
            ConsumeInputThisFrame = false;

            Debug.Log("[ActionLock] Lock released.");
        }
    }
}